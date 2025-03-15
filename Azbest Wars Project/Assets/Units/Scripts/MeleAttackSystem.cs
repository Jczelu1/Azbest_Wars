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
using static UnityEngine.RuleTile.TilingRuleOutput;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct MeleAttackSystem : ISystem
{
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
}


[BurstCompile]
public partial struct MeleAttackJob : IJobEntity
{
    [ReadOnly]
    public FlatGrid<Entity> occupied;
    [ReadOnly]
    public ComponentLookup<TeamData> teamLookup;
    public ComponentLookup<HealthData> healthLookup;
    public void Execute(Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref MeleAttackData meleAttack)
    {
        //temporary
        int team = teamLookup[entity].Team;
        if (team != 1)
            return;
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
                if (teamLookup[enemyEntity].Team != team)
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
        //make damage random
        HealthData newHealthData = healthLookup[enemyEntity];
        newHealthData.Health -= meleAttack.Damage;
        healthLookup[enemyEntity] = newHealthData;
    }
}