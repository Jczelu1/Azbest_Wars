using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class TeamAuthoring : MonoBehaviour
{
    [SerializeField]
    byte team;
    [SerializeField]
    SpriteRenderer spriteRenderer;

    private class Baker : Baker<TeamAuthoring>
    {
        public override void Bake(TeamAuthoring authoring)
        {
            authoring.spriteRenderer.color = TeamColors.GetTeamColor(authoring.team);
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TeamData { Team = authoring.team });
        }
    }
}
public struct TeamData : IComponentData
{
    public byte Team;
}
