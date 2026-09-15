using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


/// <summary>
/// Container that tracks a auto growing list of ids of type T and gives the lowest available id on request.
/// </summary>
public struct NativeIdPool
{
    public NativeArray<byte> IdList;

    public NativeIdPool(int capacity, Allocator allocator)
    {
        IdList = new NativeArray<byte>(capacity, allocator);
    }

    /// <summary>
    /// Get the first available id from the list. If none are available, the list is resized to double its previous size.
    /// </summary>
    public unsafe short RequestId()
    {
        short idCount = (short)IdList.Length;
        for (short i = 0; i < idCount; i++)
        {
            // Check for first free Id and return i if its 'unused' (0). Then set it to 'used' (1).
            if (IdList[i] == 0)
            {
                IdList[i] = 1;
                return i;
            }
        }

        NativeArray<byte> old = IdList;
        IdList = new NativeArray<byte>(idCount * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        UnsafeUtility.MemCpy(IdList.GetUnsafePtr(), old.GetUnsafeReadOnlyPtr(), old.Length);

        old.Dispose();

        // return first of newly added id and set it to 'used' (1).
        IdList[idCount] = 1;
        return idCount;
    }
    /// <summary>
    /// Return an id to the pool of available ids.
    /// </summary>
    public void ReleaseId(short id)
    {
        IdList[id] = 0;
    }
    public void SwapIds(short idA, short idB)
    {
        (IdList[idA], IdList[idB]) = (IdList[idB], IdList[idA]);
    }

    public void Dispose()
    {
        IdList.Dispose();
    }
}