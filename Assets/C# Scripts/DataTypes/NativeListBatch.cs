using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


public unsafe class NativeListBatch<T> where T : unmanaged
{
    private NativeList<T> batch1;
    private NativeList<T> batch2;

    private int cBatchId;

    public ref NativeList<T> CurrentBatch => ref (cBatchId == 0 ? ref batch1 : ref batch2);
    public ref NativeList<T> NextBatch => ref (cBatchId == 0 ? ref batch2 : ref batch1);

    public int CurrentBatchLength;


    public NativeListBatch(int startBatchSize, Allocator allocator = Allocator.Persistent)
    {
        batch1 = new NativeList<T>(startBatchSize, allocator);
        batch2 = new NativeList<T>(startBatchSize, allocator);
    }

    public void Add(T toAdd)
    {
        // Ensure there’s enough space
        if (NextBatch.Capacity < NextBatch.Length + 1)
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

    public unsafe void CycleToNextBatch()
    {
        // Ensure CurrentBatch can hold NextBatch
        if (CurrentBatch.Capacity < NextBatch.Length)
        {
            CurrentBatch.Capacity = NextBatch.Length;
        }

        CurrentBatch.Length = NextBatch.Length;

        UnsafeUtility.MemCpy(
            CurrentBatch.GetUnsafePtr(),
            NextBatch.GetUnsafePtr(),
            NextBatch.Length * UnsafeUtility.SizeOf<T>());

        // Flip batches
        cBatchId ^= 1;

        CurrentBatchLength = CurrentBatch.Length;
    }

    public void Dispose()
    {
        batch1.DisposeIfCreated();
        batch2.DisposeIfCreated();
    }
}
