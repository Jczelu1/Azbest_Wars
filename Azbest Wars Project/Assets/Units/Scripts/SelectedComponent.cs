using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SelectedAuthoring : MonoBehaviour
{
    private class Baker : Baker<SelectedAuthoring>
    {
        public override void Bake(SelectedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelectedData { Selected = false });
        }
    }
}
public struct SelectedData : IComponentData
{
    public bool Selected;
}
