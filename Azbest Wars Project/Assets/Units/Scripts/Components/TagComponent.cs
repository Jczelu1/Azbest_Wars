using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


public class HealthbarTagAuthoring : MonoBehaviour
{
    [SerializeField]
    bool healthbar;
    [SerializeField]
    bool selected;
    private class Baker : Baker<HealthbarTagAuthoring>
    {
        public override void Bake(HealthbarTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            if (authoring.healthbar)
            {
                AddComponent(entity, new HealthbarTag());
            }
            if (authoring.selected)
            {
                AddComponent(entity, new SelectedTag());
            }
        }
    }
}
public struct HealthbarTag : IComponentData { }
public struct SelectedTag : IComponentData { }
