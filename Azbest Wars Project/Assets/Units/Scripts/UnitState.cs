using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitStateAuthoring : MonoBehaviour
{
    private class Baker : Baker<UnitStateAuthoring>
    {
        public override void Bake(UnitStateAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitStateData { PathIndex = 0, Moving = false, Stuck = 0, Destination = new int2(-1, -1), PathfindState = 0 });
        }
    }
}
public struct UnitStateData : IComponentData
{
    public int PathIndex;
    public bool Moving;
    //0 - not stuck, 1 - stuck, 2 - stuck but no other path (there is nothing we can do)
    public byte Stuck;
    public int2 Destination;
    //0 - stationary, 1 - following a path, 2 - enemy search
    public byte PathfindState;
}
