using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[RequireComponent(typeof(GridPositionComponent))]
public class CaptureAreaAuthoring : MonoBehaviour
{
    [SerializeField]
    byte team;
    [SerializeField]
    int requiredCapture;
    [SerializeField]
    int width;
    [SerializeField]
    int height;
    [SerializeField]
    bool hasSpawner;

    private class Baker : Baker<CaptureAreaAuthoring>
    {
        public override void Bake(CaptureAreaAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CaptureAreaData { CapturingTeam = authoring.team,
                CaptureProgress = 0,
                RequiredCapture = authoring.requiredCapture,
                Size = new int2(authoring.width, authoring.height),
                Captured = true,
                HasSpawner = authoring.hasSpawner,
                });
        }
    }
}
public struct CaptureAreaData : IComponentData
{
    public byte CapturingTeam;
    public int RequiredCapture;
    public int CaptureProgress;
    public int2 Size;
    public bool Captured;
    public bool HasSpawner;
}
