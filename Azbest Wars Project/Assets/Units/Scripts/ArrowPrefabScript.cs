using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;



public class ArrowPrefabsAuthoring : MonoBehaviour
{
    [Header("Hit Arrow Prefabs")]
    public GameObject[] HitArrowGameObjects;
    [Header("Miss Arrow Prefabs")]
    public GameObject[] MissArrowGameObjects;
    public class ArrowPrefabsBaker : Baker<ArrowPrefabsAuthoring>
    {
        public override void Bake(ArrowPrefabsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<ArrowPrefabBuffer>(entity);

            foreach (var go in authoring.HitArrowGameObjects)
            {
                var prefabEntity = GetEntity(go, TransformUsageFlags.Dynamic);
                buffer.Add(new ArrowPrefabBuffer { Prefab = prefabEntity, IsHit = true });
            }

            foreach (var go in authoring.MissArrowGameObjects)
            {
                var prefabEntity = GetEntity(go, TransformUsageFlags.Dynamic);
                buffer.Add(new ArrowPrefabBuffer { Prefab = prefabEntity, IsHit = false });
            }
        }
    }
}


public struct ArrowPrefabBuffer : IBufferElementData
{
    public Entity Prefab;
    public bool IsHit;
}