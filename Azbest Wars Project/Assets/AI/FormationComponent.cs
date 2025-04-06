using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class FormationAuthoring : MonoBehaviour
{
    [SerializeField]
    int formation;

    private class Baker : Baker<FormationAuthoring>
    {
        public override void Bake(FormationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FormationData { Formation = authoring.formation });
        }
    }
}
public struct FormationData : IComponentData
{
    public int Formation;
}
public struct Formation
{
    public int2 Position;
    public int2 Destination;
    public Entity Objective;
    public bool IsDefending;
    public int numberOfUnits;
    public bool MoveUnits;
    public bool Completed;
}
