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
            AddComponent(entity, new PathData { PathIndex = 0, Stuck = 0, Destination = new int2(-1,-1) });
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

    //0 - not stuck, 1 - stuck, 2 - stuck but no other path (there is nothing we can do)
    public byte Stuck;
    public int2 Destination;
}
