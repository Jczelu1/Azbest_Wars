using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class Authoring : MonoBehaviour
{
    //data
    
    private class Baker : Baker<Authoring>
    {
        public override void Bake(Authoring authoring)
        {
            
        }
    }
}
public struct Component : IComponentData
{
    //data
}
