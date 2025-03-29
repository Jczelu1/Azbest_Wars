using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
[BurstCompile]
public partial class RandomValueSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref RandomValueData random) =>
        {
            float value = UnityEngine.Random.value;
            random.value = value;
            random.randDigitIndex = 0;
        }).Run();
    }
    const byte numOfRandomDigits = 6;
    public static byte Digit(float value, byte randDigitIndex)
    {
        randDigitIndex%=numOfRandomDigits;
        int valueInt = (int)(value* math.pow(10, randDigitIndex+1));
        return (byte)(valueInt % 10);
    }
}
