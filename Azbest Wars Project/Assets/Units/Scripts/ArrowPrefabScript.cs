using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class ArrowPrefabsAuthoring : MonoBehaviour
{
    public GameObject[] HitArrowGameObjects;
    public GameObject[] MissArrowGameObjects;
    public class ArrowPrefabsBaker : Baker<ArrowPrefabsAuthoring>
    {
        public override void Bake(ArrowPrefabsAuthoring authoring)
        {
            if (!ArrowSystem.HitArrowPrefabs.IsCreated)
                ArrowSystem.HitArrowPrefabs = new NativeList<Entity>(Allocator.Persistent);
            if (!ArrowSystem.MissArrowPrefabs.IsCreated)
                ArrowSystem.MissArrowPrefabs = new NativeList<Entity>(Allocator.Persistent);

            foreach (var go in authoring.HitArrowGameObjects)
            {
                var prefabEntity = GetEntity(go, TransformUsageFlags.None);
                ArrowSystem.HitArrowPrefabs.Add(prefabEntity);
            }
            foreach (var go in authoring.MissArrowGameObjects)
            {
                var prefabEntity = GetEntity(go, TransformUsageFlags.None);
                ArrowSystem.MissArrowPrefabs.Add(prefabEntity);
            }
        }
    }
}