using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct DefaultSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<Data>();
    }
    public void OnUpdate(ref SystemState state)
    {

    }
    public partial struct DefaultJob : IJobEntity
    {
        public void Execute()
        {

        }
    }
}
