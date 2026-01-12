using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


/// <summary>
/// Batch container type that allows live read writes to a NativeArray of type <typeparamref name="T"/> and syncs it to a Job only copy every <see cref="UpdateJobBatch"/>
/// </summary>
public unsafe class NativeJobBatch<T> where T : unmanaged
{
    public NativeList<T> NextBatch;
    public NativeList<T> JobBatch;
    public NativeArray<T> JobBatchAsArray() => JobBatch.AsArray();


    public NativeJobBatch(int startBatchSize, Allocator allocator = Allocator.Persistent)
    {
        JobBatch = new NativeList<T>(startBatchSize, allocator);
        NextBatch = new NativeList<T>(startBatchSize, allocator);
    }

    public void Add(T toAdd)
    {
        NextBatch.Add(toAdd);
    }
    public void RemoveAtSwapBack(int id)
    {
        NextBatch.RemoveAtSwapBack(id);
    }
    public void Set(int id, T value)
    {
        NextBatch[id] = value;
    }

    public unsafe void UpdateJobBatch()
    {
        // Ensure CurrentBatch can hold NextBatch
        if (JobBatch.Capacity < NextBatch.Length)
        {
            JobBatch.Capacity = NextBatch.Length;
        }

        JobBatch.Length = NextBatch.Length;

        UnsafeUtility.MemCpy(
            JobBatch.GetUnsafePtr(),
            NextBatch.GetUnsafePtr(),
            NextBatch.Length * UnsafeUtility.SizeOf<T>());
    }

    public void Dispose()
    {
        JobBatch.DisposeIfCreated();
        NextBatch.DisposeIfCreated();
    }
}
