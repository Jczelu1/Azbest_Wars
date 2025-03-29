using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Searcher;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct MeleAttackSystem : ISystem
{
    const float crit_Chance = .2f;
    const float block_Chance = .2f;
    private ComponentLookup<TeamData> _teamLookup;
    private ComponentLookup<HealthData> _healthLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MeleAttackData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
        _healthLookup = state.GetComponentLookup<HealthData>(false);
    }
    public void OnStartRunning(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        _teamLookup.Update(ref state);
        _healthLookup.Update(ref state);

        MeleAttackJob moveJob = new MeleAttackJob()
        {
            occupied = MainGridScript.Instance.Occupied,
            teamLookup = _teamLookup,
            healthLookup = _healthLookup,
        };
        state.Dependency = moveJob.Schedule(state.Dependency);
    }
    public partial struct MeleAttackJob : IJobEntity
    {
        [ReadOnly]
        public FlatGrid<Entity> occupied;
        [ReadOnly]
        public ComponentLookup<TeamData> teamLookup;
        public ComponentLookup<HealthData> healthLookup;
        public void Execute(Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref MeleAttackData meleAttack, ref LocalTransform transform, ref RandomValueData random)
        {
            unitState.Attacked = false;
            
            int team = teamLookup[entity].Team;
            if (unitState.Moved) return;
            int2 startPos = gridPosition.Position;

            int2 enemyPosition = new int2(-1, -1);
            Entity enemyEntity = Entity.Null;

            if (meleAttack.Range == 1)
            {
                for (int i = 0; i < 8; i++)
                {
                    int2 checkPos = startPos + Pathfinder.directions[i];
                    if (!occupied.IsInGrid(checkPos))
                        continue;
                    if (occupied[checkPos] == Entity.Null) continue;
                    enemyEntity = occupied[checkPos];
                    if (teamLookup[enemyEntity].Team != team && healthLookup.HasComponent(enemyEntity))
                    {
                        // Mark enemy as found
                        enemyPosition = checkPos;
                        break;
                    }
                }
            }
            else
            {
                //TODO
            }
            // If no enemy was found, dispose temporary collections and exit.
            if (enemyPosition.x == -1)
            {
                return;
            }
            //rotation
            if (enemyPosition.x - gridPosition.Position.x < 0)
            {
                transform.Rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (enemyPosition.x - gridPosition.Position.x > 0)
            {
                transform.Rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (enemyPosition.y - gridPosition.Position.y < 0)
            {
                transform.Rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                transform.Rotation = Quaternion.Euler(0, 0, 0);
            }
            unitState.Attacked = true;
            float damage = meleAttack.Damage;
            //block
            if (1f - random.value < block_Chance) return;
            //crit
            if (random.value < crit_Chance)
            {
                damage *= 2;
            }
            //make damage random
            HealthData newHealthData = healthLookup[enemyEntity];
            newHealthData.Health -= damage;
            newHealthData.Attacked = true;
            healthLookup[enemyEntity] = newHealthData;
            
        }
    }
}


