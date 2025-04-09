using NUnit.Framework;
using System.Linq;
using System.Net;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(SpawnerSystem))]
public partial class ArtificialIdiot : SystemBase
{
    byte groupSize = 4;
    public static int2 moveToPosition;
    public static bool move;
    int groupDelay = 20;
    int groupCountdown = 20;
    NativeList<Entity> captureAreas = new NativeList<Entity>(Allocator.Persistent);
    NativeList<Formation> formations = new NativeList<Formation>(Allocator.Persistent);
    protected override void OnCreate()
    {

    }
    protected override void OnUpdate()
    {
        byte AITeam = TeamManager.Instance.AITeam;
        //not started
        if (captureAreas.Length == 0)
        {
            Entities.WithoutBurst().ForEach((Entity entity, ref CaptureAreaData captureArea) =>
            {
                captureAreas.Add(entity);
                Debug.Log("adding area");
            }).Run();
            //Entities.WithoutBurst().ForEach((Entity entity, ref SpawnerData spawner, ref TeamData team, ref GridPosition gridPosition) =>
            //{
            //    if (team.Team != AITeam) return;
            //    spawner.SetFormation = formations.Length;
            //    formations.Add(new Formation
            //    {
            //        Position = gridPosition.Position,
            //        Destination = new int2(-1, -1),
            //        numberOfUnits = 0,
            //        IsDefending = false
            //    });
            //}).Run();
        }
        int resourceLeft = TeamManager.Instance.teamResources[AITeam];
        Entities.WithoutBurst().ForEach((Entity entity, ref SpawnerData spawner, ref GridPosition gridPosition, ref TeamData team) =>
        {
            if (team.Team != AITeam) return;
            if (spawner.Queued > 0) return;
            //queue finished
            int unitsToQueue = 1;
            int unitTypeId = 0;
            if(spawner.SetFormation == -1)
            {
                formations.Add(new Formation
                {
                    Position = gridPosition.Position,
                    Destination = new int2(-1, -1),
                    numberOfUnits = unitsToQueue,
                    IsDefending = false,
                    Objective = Entity.Null,
                    MovementState = 1,
                    Completed = false
                });
                spawner.SetFormation = formations.Length - 1;
            }
            if(resourceLeft < 0)
            {
                Formation formation = formations[spawner.SetFormation];
                formation.Completed = true;
                formations[spawner.SetFormation] = formation;
                formations.Add(new Formation
                {
                    Position = gridPosition.Position,
                    Destination = new int2(-1, -1),
                    numberOfUnits = unitsToQueue,
                    IsDefending = false,
                    Objective = Entity.Null,
                    MovementState = 1,
                    Completed = false
                });
                spawner.SetFormation = formations.Length - 1;
                return;
            }
            do
            {
                unitsToQueue = UnityEngine.Random.Range(3, 7);
                unitTypeId = UnityEngine.Random.Range(-1, SpawnerSystem.unitTypes.Length);

            } while (unitTypeId != -1 && resourceLeft < SpawnerSystem.unitTypes[unitTypeId].Cost * unitsToQueue);
            if (unitTypeId == -1)
            {
                if (spawner.SetFormation != -1)
                {
                    Formation formation = formations[spawner.SetFormation];
                    formation.Completed = true;
                    formations[spawner.SetFormation] = formation;
                }
                return;
            }
            spawner.Queued = unitsToQueue;
            spawner.SpawnedUnit = unitTypeId;
            resourceLeft -= SpawnerSystem.unitTypes[unitTypeId].Cost * unitsToQueue;
            if (UnityEngine.Random.Range(0, 2) > 0)
            {
                if (spawner.SetFormation != -1)
                {
                    Formation formation = formations[spawner.SetFormation];
                    formation.Completed = true;
                    formations[spawner.SetFormation] = formation;
                    formations.Add(new Formation
                    {
                        Position = gridPosition.Position,
                        Destination = new int2(-1, -1),
                        numberOfUnits = unitsToQueue,
                        IsDefending = false,
                        Objective = Entity.Null,
                        MovementState = 1,
                        Completed = false
                    });
                    spawner.SetFormation = formations.Length - 1;
                }
            }
        }).Run();
        for (int i = 0; i < formations.Length; i++)
        {
            if (!formations[i].Completed) continue;
            Formation formation = formations[i];
            if (UnityEngine.Random.Range(0, 2001) == 0 || (!formation.IsDefending && formation.Objective != Entity.Null && SystemAPI.GetComponent<TeamData>(formation.Objective).Team == AITeam))
            {
                int whatDo = UnityEngine.Random.Range(0, 3);
                if (whatDo == 0)
                {
                    formation.IsDefending = true;
                    formation.MovementState = 0;
                }
                if(whatDo == 1)
                {
                    formation.IsDefending = true;
                    formation.MovementState = 1;
                }
                else
                {
                    formation.Destination = new int2(-1, -1);
                }
            }
            //set destination
            if (formations[i].Destination.x == -1)
            {
                float minDistance = float.MaxValue;
                int2 moveToPos = new int2(-1, -1);
                Entity objective= Entity.Null;
                bool maxdefend = false;
                foreach (Entity e in captureAreas)
                {
                    bool defend = false;
                    if (EntityManager.GetComponentData<TeamData>(e).Team == AITeam)
                    {
                        if(UnityEngine.Random.Range(0, 2) == 0)
                        {
                            continue;
                        }
                        defend = true;
                    }
                    else if (UnityEngine.Random.Range(0, 4) == 0) continue;

                    int2 pos = EntityManager.GetComponentData<GridPosition>(e).Position;
                    pos.y -= 1;
                    float distance = math.distancesq(pos, formation.Position);

                    if (distance < minDistance && distance > 10)
                    {
                        minDistance = distance;
                        moveToPos = pos;
                        objective = e;
                        maxdefend = defend;
                    }
                }
                if (moveToPos.x == -1) continue;
                formation.IsDefending = maxdefend;
                if (formation.IsDefending)
                {
                    formation.MovementState = (byte)UnityEngine.Random.Range(0, 2);
                }
                else
                {
                    formation.MovementState = 1;
                }
                formation.Destination = moveToPos;
                formation.MoveUnits = true;
                formation.Objective = objective;
            }
            //readjust position sometimes
            if(formations[i].Destination.x != -1)
            {
                if(UnityEngine.Random.Range(0, 101) == 0)
                {
                    formation.MoveUnits = true;
                }
            }
            formations[i] = formation;
        }
        bool MovedThisTick = false;
        int MovedFormation = -2;
        Entities.WithoutBurst().ForEach((Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref TeamData team, ref SelectedData selected, ref FormationData formationData) =>
        {
            if (team.Team != AITeam) return;
            selected.Selected = false;
            if (formationData.Formation == -1 || formationData.Formation >= formations.Length) return;
            Formation formation = formations[formationData.Formation];
            if (!MovedThisTick)
            {
                if (formation.MoveUnits == true)
                {
                    Debug.Log("MoveFormation: " + formationData.Formation);
                    MovedThisTick = true;
                    MovedFormation = formationData.Formation;
                    formation.MoveUnits = false;
                    formations[formationData.Formation] = formation;
                    selected.Selected = true;
                    PathfindSystem.shouldMove[AITeam] = true;
                    PathfindSystem.destinations[AITeam] = formation.Destination;
                    PathfindSystem.setMoveState[AITeam] = formation.MovementState;
                }
            }
            else if(formationData.Formation == MovedFormation)
            {
                Debug.Log("MoveUnit");
                selected.Selected = true;
            }  
        }).Run();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        captureAreas.Dispose();
        formations.Dispose();
    }
}
