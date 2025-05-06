using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct CaptureAreaSystem : ISystem
{
    private ComponentLookup<TeamData> _teamLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CaptureAreaData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
    }
    public void OnStartRunning(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        if (!SetupSystem.started) return;
        _teamLookup.Update(ref state);
        CaptureJob captureJob = new CaptureJob()
        {
            teamLookup = _teamLookup,
            occupied = MainGridScript.Instance.Occupied,
        };
        state.Dependency = captureJob.Schedule(state.Dependency);
    }
    [BurstCompile]
    public partial struct CaptureJob : IJobEntity
    {
        const int MAX_CAP_RATE = 10;
        [ReadOnly]
        public FlatGrid<Entity> occupied;
        [ReadOnly]
        public ComponentLookup<TeamData> teamLookup;
        public void Execute(Entity entity, ref CaptureAreaData captureArea, ref GridPosition gridPosition)
        {
            int team = teamLookup[entity].Team;
            NativeArray<int> unitNumbers = new NativeArray<int>(4,Allocator.Temp);
            for (int dx = -captureArea.Size.x; dx <= captureArea.Size.x; dx++)
            {
                for (int dy = -captureArea.Size.y; dy <= captureArea.Size.y; dy++)
                {
                    int2 pos = new int2 { x = dx + gridPosition.Position.x, y = dy + gridPosition.Position.y };
                    if (occupied.IsInGrid(pos))
                    {
                        if (occupied[pos] != Entity.Null)
                        {
                            int unitTeam = teamLookup[occupied[pos]].Team;
                            unitNumbers[unitTeam]++;
                        }
                    }
                }
            }
            byte maxTeam = 255;
            int maxUnits = 0;
            for(byte i = 0; i<4; i++)
            {
                if(unitNumbers[i] > maxUnits)
                {
                    maxTeam = i;
                    maxUnits = unitNumbers[i];
                }
            }
            //if most units are from team that holds the area 
            if (maxTeam == team || maxTeam == 255)
            {
                unitNumbers.Dispose();
                captureArea.CaptureProgress = 0;
                return;
            }
            int capture = maxUnits;
            if (team < 4)
                capture -= unitNumbers[team];

            //cap the cap rate
            capture = math.min(capture, MAX_CAP_RATE);

            if (maxTeam != captureArea.CapturingTeam)
                captureArea.CaptureProgress = 0;
            captureArea.CaptureProgress += capture;
            captureArea.CapturingTeam = maxTeam;
            if(captureArea.CaptureProgress >= captureArea.RequiredCapture)
            {
                captureArea.CaptureProgress = 0;
                captureArea.Captured = true;
            }
            unitNumbers.Dispose();
        }
    }
}


