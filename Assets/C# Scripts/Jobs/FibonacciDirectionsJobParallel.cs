using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct FibonacciDirectionsJobParallel : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    [WriteOnly][NoAlias] public NativeArray<half3> directions;


    [BurstCompile]
    public void Execute(int i)
    {
        //if (i != 0) return;

        //directions[0] = new half3((half)0.2f, (half)0, (half)1);
        //directions[1] = new half3((half)0.25f, (half)0, (half)1);
        //directions[2] = new half3((half)0.3f, (half)0, (half)1);

        //return;

        int count = directions.Length;
        float phi = math.PI * (3f - math.sqrt(5f)); // golden angle in radians
        float y = 1f - (i / (float)(count - 1)) * 2f;
        float radius = math.sqrt(1f - y * y);
        float theta = phi * i;

        float x = math.cos(theta) * radius;
        float z = math.sin(theta) * radius;

        directions[i] = (half3)new float3(x, y, z);
    }
}