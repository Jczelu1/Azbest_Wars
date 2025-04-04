using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateAfter(typeof(MoveSystem))]
//public partial struct FogOfWarSystem : ISystem
//{
//    private ComponentLookup<TeamData> _teamLookup;
//    public void OnCreate(ref SystemState state)
//    {
//        _teamLookup = state.GetComponentLookup<TeamData>(true);
//    }

//    public void OnUpdate(ref SystemState state)
//    {
//        _teamLookup.Update(ref state);
//        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
//        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
//        var ecbSystem = World.DefaultGameObjectInjectionWorld
//            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

//        SpotJob spotJob = new SpotJob()
//        {
//            occupied = MainGridScript.Instance.Occupied,
//            ecb = ecb,
//            playerTeam = MainGridScript.Instance.PlayerTeam,
//            teamLookup = _teamLookup
//        };

//        JobHandle pathJobHandle = spotJob.ScheduleParallel(state.Dependency);

//        state.Dependency = pathJobHandle;
//        pathJobHandle.Complete();
//        //Ensure ECB commands are played back safely
//        ecbSystem.Update(state.WorldUnmanaged);
//    }
//    [BurstCompile]
//    public partial struct SpotJob : IJobEntity
//    {
//        [ReadOnly]
//        public FlatGrid<Entity> occupied;
//        [ReadOnly]
//        public ComponentLookup<TeamData> teamLookup;
//        public int playerTeam;
//        public EntityCommandBuffer.ParallelWriter ecb;
//        public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref SpotterData spotter, ref GridPosition gridPosition, ref UnitStateData unitState)
//        {
//            int team = teamLookup[entity].Team;
//            if(team!=playerTeam) return;
//            for (int dx = -spotter.SpotRange; dx <= spotter.SpotRange; dx++)
//            {
//                for (int dy = -spotter.SpotRange; dy <= spotter.SpotRange; dy++)
//                {
//                    int2 pos = new int2 { x = dx + gridPosition.Position.x, y = dy + gridPosition.Position.y };
//                    if (occupied.IsInGrid(pos))
//                    {
//                        if (occupied[pos] != Entity.Null && teamLookup[occupied[pos]].Team!=playerTeam)
//                        {
//                            //might not work
//                            ecb.SetComponent(sortKey, occupied[pos], new VisibleData { Visible = true, SetVisible = true });
//                        }
//                    }
//                }
//            }
//        }
//    }

//    public partial struct VisibleJob : IJobEntity
//    {
//        public ComponentLookup<TeamData> teamLookup;
//        public int playerTeam;
//        public void Execute(Entity entity, ref VisibleData visible, SpriteRenderer renderer,  in DynamicBuffer<Child> children)
//        {
//            int team = teamLookup[entity].Team;
//            if (team == playerTeam)
//            {
//                renderer.enabled = true;
//                return;
//            }
//            renderer.enabled = visible.Visible;
//            visible.Visible = false;
//        }
//    }
//}
