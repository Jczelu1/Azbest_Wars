using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PathAuthoring : MonoBehaviour
{
    private class Baker : Baker<PathAuthoring>
    {
        public override void Bake(PathAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<PathNode>(entity);
            AddComponent(entity, new PathData { PathIndex = 0 });
        }
    }
}
//change later
[InternalBufferCapacity(0)]
public struct PathNode : IBufferElementData
{
    public int2 PathPos;
}

public struct PathData : IComponentData
{
    public int PathIndex;
}
