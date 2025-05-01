using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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
            if (unitState.Moved)
            {
                if(meleAttack.MoveCooldown != 255)
                {
                    meleAttack.CurrentCooldown = meleAttack.MoveCooldown;
                    return;
                }
            }
            if(meleAttack.CurrentCooldown != 0)
            {
                meleAttack.CurrentCooldown--;
                return;
            }
                
            int2 startPos = gridPosition.Position;

            if (meleAttack.AttackType == 0)
            {
                int2 enemyPosition = new int2(-1, -1);
                Entity enemyEntity = Entity.Null;
                for (int i = 0; i < 8; i++)
                {
                    int2 checkPos = startPos + Pathfinder.directions[i];
                    if (!occupied.IsInGrid(checkPos))
                        continue;
                    if (occupied[checkPos] == Entity.Null) continue;
                    enemyEntity = occupied[checkPos];
                    if (teamLookup[enemyEntity].Team != team)
                    {
                        // Mark enemy as found
                        enemyPosition = checkPos;
                        break;
                    }
                }
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
                meleAttack.CurrentCooldown = meleAttack.AttackCooldown;
                float damage = meleAttack.Damage;
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
            }
            else if(meleAttack.AttackType == 1)
            {
                int2x3 enemyPositions = new int2x3(-1);
                int maxIndex = -1;
                int maxNumber = 0;
                for (int i = 0; i < 8; i++)
                {
                    int currentNumber = 0;
                    int2 checkPos;
                    checkPos = startPos + AreaAttackDirections[i].c0;
                    if (occupied.IsInGrid(checkPos) && occupied[checkPos] != Entity.Null && teamLookup[occupied[checkPos]].Team != team)
                    {
                        currentNumber++;
                    }
                    checkPos = startPos + AreaAttackDirections[i].c1;
                    if (occupied.IsInGrid(checkPos) && occupied[checkPos] != Entity.Null && teamLookup[occupied[checkPos]].Team != team)
                    {
                        currentNumber++;
                    }
                    checkPos = startPos + AreaAttackDirections[i].c2;
                    if (occupied.IsInGrid(checkPos) && occupied[checkPos] != Entity.Null && teamLookup[occupied[checkPos]].Team != team)
                    {
                        currentNumber++;
                    }
                    if(currentNumber > maxNumber)
                    {
                        maxIndex = i;
                        maxNumber = currentNumber;
                    }
                }
                if (maxIndex == -1)
                {
                    return;
                }
                enemyPositions = AreaAttackDirections[maxIndex];
                //rotation
                if (enemyPositions.c0.x < 0)
                {
                    transform.Rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (enemyPositions.c0.x > 0)
                {
                    transform.Rotation = Quaternion.Euler(0, 180, 0);
                }
                else if (enemyPositions.c0.y < 0)
                {
                    transform.Rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    transform.Rotation = Quaternion.Euler(0, 0, 0);
                }
                unitState.Attacked = true;
                meleAttack.CurrentCooldown = meleAttack.AttackCooldown;
                float damage = meleAttack.Damage;
                //block
                //if (1f - random.value < block_Chance) return;
                //crit
                if (random.value < crit_Chance)
                {
                    damage *= 2;
                }
                Entity enemyEntity;
                HealthData newHealthData;

                if (occupied.IsInGrid(enemyPositions.c0 + startPos))
                {
                    enemyEntity = occupied[enemyPositions.c0 + startPos];
                    if (enemyEntity != Entity.Null && teamLookup[enemyEntity].Team != team)
                    {
                        newHealthData = healthLookup[enemyEntity];
                        newHealthData.Health -= damage;
                        newHealthData.Attacked = true;
                        healthLookup[enemyEntity] = newHealthData;
                    }
                }

                if (occupied.IsInGrid(enemyPositions.c1 + startPos))
                {
                    enemyEntity = occupied[enemyPositions.c1 + startPos];
                    if (enemyEntity != Entity.Null && teamLookup[enemyEntity].Team != team)
                    {

                        newHealthData = healthLookup[enemyEntity];
                        newHealthData.Health -= damage;
                        newHealthData.Attacked = true;
                        healthLookup[enemyEntity] = newHealthData;
                    }
                }

                if (occupied.IsInGrid(enemyPositions.c2 + startPos))
                {
                    enemyEntity = occupied[enemyPositions.c2 + startPos];
                    if (enemyEntity != Entity.Null && teamLookup[enemyEntity].Team != team)
                    {
                        newHealthData = healthLookup[enemyEntity];
                        newHealthData.Health -= damage;
                        newHealthData.Attacked = true;
                        healthLookup[enemyEntity] = newHealthData;
                    }
                }
            }
            else if (meleAttack.AttackType == 2)
            {
                int2 enemyPosition = new int2(-1, -1);
                Entity enemyEntity = Entity.Null;
                for (int i = 0; i < 24; i++)
                {
                    int2 checkPos = startPos + Range2AttackDirections[i];
                    if (!occupied.IsInGrid(checkPos))
                        continue;
                    if (occupied[checkPos] == Entity.Null) continue;
                    enemyEntity = occupied[checkPos];
                    if (teamLookup[enemyEntity].Team != team)
                    {
                        // Mark enemy as found
                        enemyPosition = checkPos;
                        break;
                    }
                }
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
                meleAttack.CurrentCooldown = meleAttack.AttackCooldown;
                float damage = meleAttack.Damage;
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
            }
        }
    }
}


