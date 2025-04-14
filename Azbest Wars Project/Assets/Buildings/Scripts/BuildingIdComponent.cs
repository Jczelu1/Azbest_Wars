using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BuildingIdAuthoring : MonoBehaviour
{
    [SerializeField]
    int id;
    [SerializeField]
    bool isSpawner;
    [SerializeField]
    bool isArea;

    private class Baker : Baker<BuildingIdAuthoring>
    {
        public override void Bake(BuildingIdAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BuildingIdData { Id = authoring.id, IsArea = authoring.isArea, IsSpawner = authoring.isSpawner });
        }
    }
}
public struct BuildingIdData : IComponentData
{
    public int Id;
    public bool IsSpawner;
    public bool IsArea;
}
