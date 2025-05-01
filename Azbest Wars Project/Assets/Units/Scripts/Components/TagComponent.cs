using UnityEngine;
using Unity.Entities;


public class HealthbarTagAuthoring : MonoBehaviour
{
    [SerializeField]
    bool healthbar;
    [SerializeField]
    bool selected;
    [SerializeField]
    bool interestPoint;
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
            if (authoring.interestPoint)
            {
                AddComponent(entity, new InterestPointTag());
            }
        }
    }
}
public struct HealthbarTag : IComponentData { }
public struct SelectedTag : IComponentData { }
public struct InterestPointTag : IComponentData { }
