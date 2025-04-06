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
[UpdateBefore(typeof(PathfindSystem))]
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
        Entities.WithoutBurst().ForEach((Entity entity, ref SpawnerData spawner, ref GridPosition gridPosition, ref TeamData team) =>
        {
            if (team.Team != AITeam) return;
            if (spawner.Queued > 0) return;
            if (spawner.SetFormation != -1)
            {
                Formation formation = formations[spawner.SetFormation];
                formation.Completed = true;
                formations[spawner.SetFormation] = formation;
            }
            //queue finished
            int unitsToQueue = UnityEngine.Random.Range(6, 13);
            //make random when more unit types
            int unitTypeId = 0;
            spawner.Queued = unitsToQueue;
            spawner.SpawnedUnit = unitTypeId;
            spawner.SetFormation = formations.Length;
            
            formations.Add(new Formation
            {
                Position = gridPosition.Position,
                Destination = new int2(-1, -1),
                numberOfUnits = unitsToQueue,
                IsDefending = false,
                Objective = Entity.Null
            });
        }).Run();
        for (int i = 0; i < formations.Length; i++)
        {
            Formation formation = formations[i];
            if (!formation.IsDefending && formation.Objective != Entity.Null && SystemAPI.GetComponent<TeamData>(formation.Objective).Team == AITeam)
            {
                //defend
                if (UnityEngine.Random.Range(0, 2) > 0)
                {
                    formation.IsDefending = true;
                }
                //attack
                else
                {
                    formation.Destination = new int2(-1, -1);
                }
            }
            //set destination
            if (formations[i].Destination.x == -1 && formations[i].Completed)
            {
                float minDistance = float.MaxValue;
                int2 moveToPos = new int2(-1, -1);
                Entity objective= Entity.Null;

                foreach (Entity e in captureAreas)
                {
                    if (EntityManager.GetComponentData<TeamData>(e).Team == AITeam) continue;

                    int2 pos = EntityManager.GetComponentData<GridPosition>(e).Position;
                    pos.y -= 1;
                    float distance = math.distancesq(pos, formation.Position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        moveToPos = pos;
                        objective = e;
                    }
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
        int MovedFormation = -1;
        Entities.WithoutBurst().ForEach((Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref TeamData team, ref SelectedData selected, ref FormationData formationData) =>
        {
            if (team.Team != AITeam) return;
            if (formationData.Formation == -1 || formationData.Formation >= formations.Length) return;

            if (!MovedThisTick)
            {
                Formation formation = formations[formationData.Formation];
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
                    PathfindSystem.setMoveState[AITeam] = 1;
                }
            }
            else if(formationData.Formation == MovedFormation)
            {
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
