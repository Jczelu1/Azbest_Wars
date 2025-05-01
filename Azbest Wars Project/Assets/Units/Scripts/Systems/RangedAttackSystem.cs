using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Arrow
{
    public Entity Target;
    public byte Team;
    public float Damage;
    public byte TimeToHit;
    public bool Miss;
}

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
[UpdateBefore(typeof(MeleAttackSystem))]
public partial struct RangedAttackSystem : ISystem
{
    const float HIT_CHANCE_D1 = .8f;
    const float HIT_CHANCE_D2 = .6f;
    public NativeList<Arrow> ShotArrows;
    public static NativeList<Arrow> SpawnArrows;
    private ComponentLookup<TeamData> _teamLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MeleAttackData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
        ShotArrows = new NativeList<Arrow>(Allocator.Persistent);
        SpawnArrows = new NativeList<Arrow>(Allocator.Persistent);
    }
    public void OnStartRunning(ref SystemState state)
    {

    }
    public void OnDestroy(ref SystemState state)
    {
        ShotArrows.Dispose();
        SpawnArrows.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        SpawnArrows.Clear();
        foreach (var entity in ArrowSystem.SpawnedArrows)
        {
            state.EntityManager.DestroyEntity(entity);
        }
        ArrowSystem.SpawnedArrows.Clear();
        for (int i = ShotArrows.Length - 1; i >= 0; i--)
        {
            var arrow = ShotArrows[i];
            arrow.TimeToHit--;

            if (arrow.TimeToHit <= 0)
            {
                if (!arrow.Miss && state.EntityManager.HasComponent<HealthData>(arrow.Target))
                {
                    var healthData = state.EntityManager.GetComponentData<HealthData>(arrow.Target);
                    healthData.Health -= arrow.Damage;
                    healthData.Attacked = true;
                    state.EntityManager.SetComponentData(arrow.Target, healthData);
                }

                ShotArrows.RemoveAt(i);
                SpawnArrows.Add(arrow);
            }
            else
            {
                ShotArrows[i] = arrow;
            }
        }
        _teamLookup.Update(ref state);

        RangedAttackJob Job = new RangedAttackJob()
        {
            occupied = MainGridScript.Instance.Occupied,
            teamLookup = _teamLookup,
            shotArrows = ShotArrows,
        };
        state.Dependency = Job.Schedule(state.Dependency);
    }
    public partial struct RangedAttackJob : IJobEntity
    {
        [ReadOnly]
        public FlatGrid<Entity> occupied;
        [ReadOnly]
        public ComponentLookup<TeamData> teamLookup;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeList<Arrow> shotArrows;
        public void Execute(Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref RangedAttackData rangedAttack, ref LocalTransform transform, ref RandomValueData random)
        {
            unitState.Attacked = false;

            byte team = teamLookup[entity].Team;
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

            bool miss = false;
            
            int distance = math.max(math.abs(gridPosition.Position.x - enemyPosition.x), math.abs(gridPosition.Position.y - enemyPosition.y));
            //distance 1
            if (distance < rangedAttack.Range / 2)
            {
                if (random.value > HIT_CHANCE_D1)
                    miss = true;
                shotArrows.Add(new Arrow { Damage = damage, Miss = miss, Target = enemyEntity, TimeToHit = 1, Team = team });
            }
            //distance 2
            else
            {
                if (random.value > HIT_CHANCE_D2)
                    miss = true;
                shotArrows.Add(new Arrow { Damage = damage, Miss = miss, Target = enemyEntity, TimeToHit = 1, Team = team });
            }


            queue.Dispose();
            searched.Dispose();
        }
    }
}

