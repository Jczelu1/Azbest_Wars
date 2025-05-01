using UnityEngine;
using Unity.Entities;

public class TeamAuthoring : MonoBehaviour
{
    [SerializeField]
    byte team;

    private class Baker : Baker<TeamAuthoring>
    {
        public override void Bake(TeamAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TeamData { Team = authoring.team });
        }
    }
}
public struct TeamData : IComponentData
{
    public byte Team;
}
