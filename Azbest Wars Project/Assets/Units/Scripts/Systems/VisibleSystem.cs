using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(FogOfWarSystem))]
public partial class VisibleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var teamLookup = GetComponentLookup<TeamData>(isReadOnly: true);
        int playerTeam = MainGridScript.Instance.PlayerTeam;

        Entities.WithoutBurst().ForEach((Entity entity, ref VisibleData visible, in DynamicBuffer<Child> children) =>
        {
            int team = teamLookup[entity].Team;

            if (team == playerTeam)
            {
                return;
            }

            
            var spriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(entity);
            spriteRenderer.enabled = visible.SetVisible;
            foreach (var child in children)
            {
                if (EntityManager.HasComponent<SpriteRenderer>(child.Value) && EntityManager.HasComponent<HealthbarTag>(child.Value))
                {
                    var childSpriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                    childSpriteRenderer.enabled = visible.SetVisible;
                }
            }
            visible.Visible = visible.SetVisible;
            visible.SetVisible = false;
        }).Run();
    }
}
