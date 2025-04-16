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
public partial struct RangedAttackSystem : ISystem
{
    const float crit_Chance = .2f;
    const float block_Chance = .2f;
    private ComponentLookup<TeamData> _teamLookup;
    private ComponentLookup<HealthData> _healthLookup;

    //efficiency xd
    public static readonly int2x3[] AreaAttackDirections = new int2x3[8]
    {
        new int2x3(new int2(0,-1), new int2(1,-1),new int2(-1,-1)),
        new int2x3(new int2(0,1), new int2(1,1),new int2(-1,1)),
        new int2x3(new int2(-1,0), new int2(-1,1),new int2(-1,-1)),
        new int2x3(new int2(1,0), new int2(1,1),new int2(1,-1)),
        new int2x3(new int2(-1,1), new int2(0,1),new int2(-1,0)),
        new int2x3(new int2(1,1), new int2(0,1),new int2(1,0)),
        new int2x3(new int2(-1,-1), new int2(-1,0),new int2(0,-1)),
        new int2x3(new int2(1,-1), new int2(1,0),new int2(0,-1)),
    };
    //efficiency xddd
    public static readonly int2[] Range2AttackDirections = new int2[24]
    {
        new int2(0, -1),   new int2(0, 1),
        new int2(-1, 0),  new int2(1, 0),
        new int2(-1, 1),  new int2(1, 1),
        new int2(-1, -1), new int2(1, -1),
        new int2(0,-2), new int2(0, 2),
        new int2(-2,0), new int2(2,0),
        new int2(-1,-2), new int2(-1,2),
        new int2(1,-2), new int2(1,2),
        new int2(-2,1), new int2(2,1),
        new int2(-2,-1), new int2(2,-1),
        new int2(-2,2), new int2(2,2),
        new int2(-2,-2), new int2(2,-2),
    };
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

        RangedAttackJob Job = new RangedAttackJob()
        {
            occupied = MainGridScript.Instance.Occupied,
            teamLookup = _teamLookup,
            healthLookup = _healthLookup,
        };
        state.Dependency = Job.Schedule(state.Dependency);
    }
    public partial struct RangedAttackJob : IJobEntity
    {
        [ReadOnly]
        public FlatGrid<Entity> occupied;
        [ReadOnly]
        public ComponentLookup<TeamData> teamLookup;
        public ComponentLookup<HealthData> healthLookup;
        public void Execute(Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref RangedAttackData rangedAttack, ref LocalTransform transform, ref RandomValueData random)
        {
            unitState.Attacked = false;

            int team = teamLookup[entity].Team;
            if (unitState.Moved)
            {
                if (rangedAttack.MoveCooldown != 255)
                {
                    rangedAttack.CurrentCooldown = rangedAttack.MoveCooldown;
                    return;
                }
            }
            if (rangedAttack.CurrentCooldown != 0)
            {
                rangedAttack.CurrentCooldown--;
                return;
            }

            int2 startPos = gridPosition.Position;
            // Maximum possible number of nodes in the search area
            int maxNodes = (rangedAttack.Range * 2 + 1) * (rangedAttack.Range * 2 + 1);
            NativeList<int2> queue = new NativeList<int2>(maxNodes, Allocator.Temp);
            NativeHashMap<int2, int2> searched = new NativeHashMap<int2, int2>(maxNodes, Allocator.Temp);

            // Initialize the search with the starting position.
            queue.Add(startPos);
            searched.Add(startPos, new int2(-1, -1));

            int2 enemyPosition = new int2(-1, -1);
            Entity enemyEntity = Entity.Null;
            bool foundEnemy = false;
            int head = 0; // Use a head pointer for FIFO behavior

            // Breadth-first search loop.
            while (head < queue.Length && !foundEnemy)
            {
                int2 current = queue[head];
                head++;

                for (int i = 0; i < Pathfinder.directions.Length; i++)
                {
                    int2 offset = Pathfinder.directions[i];
                    int2 neighbor = current + offset;

                    // Skip if already visited.
                    if (searched.ContainsKey(neighbor))
                        continue;

                    // Skip if outside grid bounds.
                    if (!occupied.IsInGrid(neighbor))
                        continue;

                    // Ensure neighbor is within the auto-find search radius.
                    if (math.abs(neighbor.x - startPos.x) > rangedAttack.Range ||
                        math.abs(neighbor.y - startPos.y) > rangedAttack.Range)
                        continue;

                    //maybe also continue if tile also occupied by other unit
                    if (occupied[neighbor] != Entity.Null && occupied[neighbor] != entity)
                    {
                        enemyEntity = occupied[neighbor];
                        // Check if the enemy is on a different team.
                        if (teamLookup[enemyEntity].Team != team)
                        {
                            // Mark enemy as found
                            enemyPosition = neighbor;
                            searched.Add(neighbor, current);
                            foundEnemy = true;
                            break;
                        }
                        else if (unitState.Stuck == 1) continue;
                    }

                    // Enqueue valid neighbor.
                    queue.Add(neighbor);
                    searched.Add(neighbor, current);
                }
            }

            // If no enemy was found, dispose temporary collections and exit.
            if (enemyPosition.x == -1)
            {
                queue.Dispose();
                searched.Dispose();
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
            rangedAttack.CurrentCooldown = rangedAttack.AttackCooldown;
            float damage = rangedAttack.Damage;
            //block
            //if (1f - random.value < block_Chance) return;
            //crit
            if (random.value < crit_Chance)
            {
                damage *= 2;
            }
            HealthData newHealthData = healthLookup[enemyEntity];
            newHealthData.Health -= damage;
            newHealthData.Attacked = true;
            healthLookup[enemyEntity] = newHealthData;

            queue.Dispose();
            searched.Dispose();
        }
    }
}

