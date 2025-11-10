using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


public class NativeListBatch<T> where T : unmanaged
{
    private NativeList<T> batch1;
    private NativeList<T> batch2;

    private int cBatchId;

    public ref NativeList<T> CurrentBatch => ref (cBatchId == 0 ? ref batch1 : ref batch2);
    public ref NativeList<T> NextBatch => ref (cBatchId == 0 ? ref batch2 : ref batch1);


    public NativeListBatch(int startBatchSize, Allocator allocator = Allocator.Persistent)
    {
        batch1 = new NativeList<T>(startBatchSize, allocator);
        batch2 = new NativeList<T>(startBatchSize, allocator);
    }

    public void Add(T toAdd)
    {
        // Ensure there’s enough space
        if (NextBatch.Capacity == NextBatch.Length)
        {
            NextBatch.Capacity = math.max(1, NextBatch.Length * 2);
        }
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

    public void CycleToNextBatch()
    {
        NativeList<T> oldBatch = cBatchId == 0 ? batch1 : batch2;
        NativeList<T> newBatch = cBatchId == 0 ? batch2 : batch1;

        if (newBatch.Capacity != oldBatch.Capacity)
        {
            oldBatch.Capacity = newBatch.Capacity;
        }

        // Create and Instantly complete Copy Job
        // Copy new collider array into old array
        // Afterward "old" arraybecomes new array
        new ArrayCopyJob<T>()
        {
            destination = oldBatch.AsArray(),
            source = newBatch.AsArray()
        }.Run();

        cBatchId ^= 1; // Flip between 0 and 1
    }

    public void Dispose()
    {
        batch1.DisposeIfCreated();
        batch2.DisposeIfCreated();
    }
}
