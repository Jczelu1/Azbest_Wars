using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitStateAuthoring : MonoBehaviour
{
    [SerializeField]
    byte MovementState = 0;
    private class Baker : Baker<UnitStateAuthoring>
    {
        public override void Bake(UnitStateAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitStateData { PathIndex = 0, Moved = false,  Attacked = false, Stuck = 0, Destination = new int2(-1, -1), MovementState = authoring.MovementState, MoveProcessed = false });
            AddComponent(entity, new VisibleData { Visible = false, SetVisible = false });
        }
    }
}
public struct UnitStateData : IComponentData
{
    public int PathIndex;
    public bool Moved;
    public bool Attacked;
    public bool MoveProcessed;
    //0 - not stuck, 1 - stuck, 2 - stuck but no other path (there is nothing we can do)
    public byte Stuck;
    public int2 Destination;
    //0 - defend/stationary, 1 - attack, 2 - retreat
    public byte MovementState;
}
public struct VisibleData : IComponentData
{
    public bool Visible;
    public bool SetVisible;
}
