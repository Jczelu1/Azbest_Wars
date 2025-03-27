using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class CaptureSystem : SystemBase
{    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((Entity entity, ref CaptureAreaData captureArea, ref TeamData team) =>
        {
            if (!captureArea.Captured) return;
            captureArea.Captured = false;
            if (captureArea.CapturingTeam>3) return;
            team.Team = captureArea.CapturingTeam;
            EntityManager.GetComponentObject<SpriteRenderer>(entity).color = TeamColors.GetTeamColor(team.Team);

            //foreach (var child in children)
            //{
            //    if (EntityManager.HasComponent<SpriteRenderer>(child.Value))
            //    {
            //        EntityManager.GetComponentObject<SpriteRenderer>(child.Value).color = TeamColors.GetTeamColor(team.Team);
            //        EntityManager.SetComponentData<TeamData>(child.Value, team);
            //    }
            //}
        }).Run();
    }
}

