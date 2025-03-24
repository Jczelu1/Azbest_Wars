using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class CaptureAreaAuthoring : MonoBehaviour
{
    [SerializeField]
    byte team;
    [SerializeField]
    int requiredCapture;

    private class Baker : Baker<CaptureAreaAuthoring>
    {
        public override void Bake(CaptureAreaAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CaptureAreaData { CurrentTeam = authoring.team, CaptureProgress = 0, RequiredCapture = authoring.requiredCapture });
        }
    }
}
public struct CaptureAreaData : IComponentData
{
    public byte CurrentTeam;
    public int RequiredCapture;
    public int CaptureProgress;
}
