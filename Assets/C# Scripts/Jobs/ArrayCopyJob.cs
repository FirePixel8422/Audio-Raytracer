using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;


[BurstCompile(DisableSafetyChecks = true)]
public struct ArrayCopyJob<T> : IJob
    where T : unmanaged
{
    [NoAlias][ReadOnly] public NativeArray<T> source;

    [NoAlias][WriteOnly] public NativeArray<T> destination;


    [BurstCompile(DisableSafetyChecks = true)]
    public void Execute()
    {
        int length = source.Length;
        for (int i = 0; i < length; i++)
        {
            destination[i] = source[i];
        }
    }
}