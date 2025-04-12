//using System.Threading;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.VisualScripting;

//[BurstCompile]
//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateBefore(typeof(TickManagerSystem))]
//public partial struct UnitCounterSystem : ISystem
//{
//    public static NativeArray<int> unitCounts;
//    public static NativeArray<int> unitSelectedCounts;
//    public static NativeArray<int> unitSelectedTypes;

//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<UnitStateData>();
//        state.RequireForUpdate<LocalTransform>();
//        unitCounts = new NativeArray<int>(4,Allocator.Persistent);
//        unitSelectedCounts = new NativeArray<int>(4, Allocator.Persistent);
//        unitSelectedTypes = new NativeArray<int>(4, Allocator.Persistent);
//    }
//    public void OnUpdate(ref SystemState state)
//    {
//        for(int i = 0; i < 4; i++)
//        {
//            unitCounts[i] = 0;
//            unitSelectedCounts[i] = 0;
//            unitSelectedTypes[i] = -2;
//        }
//        CountJob Job = new CountJob()
//        {
//            unitCounts = unitCounts,
//            unitSelectedCounts = unitSelectedCounts,
//            unitSelectedTypes = unitSelectedTypes,
//        };
//        state.Dependency = Job.Schedule(state.Dependency);
//    }
//    public void OnDestroy(ref SystemState state)
//    {
//        unitCounts.Dispose();
//        unitSelectedCounts.Dispose();
//        unitSelectedTypes.Dispose();
//    }
//}


//[BurstCompile]
//public partial struct CountJob : IJobEntity
//{
//    public NativeArray<int> unitCounts;
//    public NativeArray<int> unitSelectedCounts;
//    public NativeArray<int> unitSelectedTypes;

//    public void Execute(in UnitStateData unitState, in TeamData team, in SelectedData selected, ref UnitTypeId typeId)
//    {
//        byte teamIndex = team.Team;

//        unitCounts[teamIndex] +=1;

//        if (selected.Selected)
//        {
//            unitSelectedCounts[teamIndex] += 1;
//            if (unitSelectedTypes[teamIndex] == -2)
//            {
//                unitCounts[teamIndex] = typeId.Id; 
//            }
//            else if(unitSelectedTypes[teamIndex] != typeId.Id)
//            {
//                unitCounts[teamIndex] = -1;
//            }
//        }
//    }
//}