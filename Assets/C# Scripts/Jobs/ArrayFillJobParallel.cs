using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;


[BurstCompile(DisableSafetyChecks = true)]
public struct IntArrayFillJobParallel : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    [WriteOnly][NoAlias] public NativeArray<int> array;

    [WriteOnly][NoAlias] public int value;


    [BurstCompile(DisableSafetyChecks = true)]
    public void Execute(int index)
    {
        array[index] = value;
    }
}