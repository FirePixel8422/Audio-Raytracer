using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;



[BurstCompile]
public class AudioColliderManager : MonoBehaviour
{
    public static AudioColliderManager Instance { get; private set; }


    private List<AudioCollider> colliders;

    /// <summary>
    /// Whether colliderArray[0] or [1] is NOT used by job atm
    /// targetArray[NextBatchId] is always the array thats allowed to be modified
    /// </summary>
    public int NextBatchId { get; private set; }
    public const int BatchCount = 2;

    public void CycleToNextBatch()
    {
        int currentBatchId = NextBatchId;

        NextBatchId += 1;
        if (NextBatchId == BatchCount)
        {
            NextBatchId = 0;
        }

        if (AABBCount >= AABBColliders.Length)


            AABBColliders[currentBatchId].CopyTo(AABBColliders[NextBatchId]);
    }


    public NativeArray<ColliderAABBStruct>[] AABBColliders;
    private short aabbCount;
    public short AABBCount => aabbCount;


    public NativeArray<ColliderOBBStruct>[] OBBColliders;
    private short obbCount;
    public short OBBCount => obbCount;


    public NativeArray<ColliderSphereStruct>[] SphereColliders;
    private short sphereCount;
    public short SphereCount => sphereCount;



    private void Awake()
    {
        Instance = this;

        // Fetch all AudioColliders in Scene
        colliders = this.FindObjectsOfType<AudioCollider>().ToList();
        int colliderCount = colliders.Count;

        DebugLogger.Throw("CRTICAL ERROR: The audio raytracer does NOT support more then more then 32767 audio colliders, audio system crashed", colliderCount > short.MaxValue);

        // Get total amount of colliders for box (AABB, OBB) and spheres
        for (int i = 0; i < colliderCount; i++)
        {
            switch (colliders[i].GetColliderType())
            {
                case ColliderType.Sphere:
                    sphereCount += 1;
                    break;

                case ColliderType.AABB:
                    aabbCount += 1;
                    break;

                case ColliderType.OBB:
                    obbCount += 1;
                    break;

                default:
                    DebugLogger.LogError("Null Collider detected");
                    break;
            }
        }


        // Setup native arrays for colliders
        for (int i = 0; i < BatchCount; i++)
        {
            AABBColliders[i] = new NativeArray<ColliderAABBStruct>(aabbCount, Allocator.Persistent);
            OBBColliders[i] = new NativeArray<ColliderOBBStruct>(obbCount, Allocator.Persistent);
            SphereColliders[i] = new NativeArray<ColliderSphereStruct>(sphereCount, Allocator.Persistent);
        }

        for (short i = 0; i < colliderCount; i++)
        {
            CheckForResizing();
            colliders[i].AddToAudioSystem(ref AABBColliders[NextBatchId], ref aabbCount, ref OBBColliders[NextBatchId], ref obbCount, ref SphereColliders[NextBatchId], ref sphereCount, i);
        }
    }

    public void AddColiderToSystem(AudioCollider targetCollider)
    {
        CheckForResizing();

        targetCollider.AddToAudioSystem(ref AABBColliders[NextBatchId], ref aabbCount, ref OBBColliders[NextBatchId], ref obbCount, ref SphereColliders[NextBatchId], ref sphereCount, (short)colliders.Count);
        colliders.Add(targetCollider);
    }
    public void RemoveColiderFromSystem(AudioCollider targetCollider)
    {
        short toRemoveColliderId = targetCollider.AudioColliderId;

        // RemoveAtSwapBack functionality
        colliders[^1].AudioColliderId = toRemoveColliderId;

        //
        colliders.RemoveAtSwapBack(toRemoveColliderId);
        // Swap last collider to the "to remove" collider's spot, and set the last collider to null
        colliders[toRemoveColliderId] = colliders[^1];
    }

    private void CheckForResizing()
    {
        if (AABBCount == AABBColliders.Length)
        {
            ResizeDoubleCapacity(ref AABBColliders[NextBatchId], Allocator.Persistent);
        }
        if (OBBCount == OBBColliders.Length)
        {
            ResizeDoubleCapacity(ref OBBColliders[NextBatchId], Allocator.Persistent);
        }
        if (sphereCount == SphereColliders.Length)
        {
            ResizeDoubleCapacity(ref SphereColliders[NextBatchId], Allocator.Persistent);
        }
    }

    [BurstCompile]
    private static void ResizeDoubleCapacity<T>(ref NativeArray<T> array, Allocator allocator) where T : unmanaged
    {
        int newLength = math.max(array.Length * 2, 1);

        NativeArray<T> newArray = new NativeArray<T>(newLength, allocator, NativeArrayOptions.UninitializedMemory);
        NativeArray<T>.Copy(array, newArray, math.min(array.Length, newLength));

        array.Dispose();
        array = newArray;
    }

    private void OnDestroy()
    {
        UpdateScheduler.CreateLateOnApplicationQuitCallback(Dispose);
    }

    private void Dispose()
    {
        AABBColliders.DisposeIfCreated();
        OBBColliders.DisposeIfCreated();
        SphereColliders.DisposeIfCreated();
    }


#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField] private bool drawColliderGizmos = true;
    [SerializeField] private Color colliderGizmosColor = new Color(1f, 0.75f, 0.25f);
    public readonly static Color ColliderGizmosSelectedColor = new Color(1f, 0.75f, 0.25f);

    private void OnDrawGizmos()
    {
        AudioCollider[] colliders = this.FindObjectsOfType<AudioCollider>();

        //green blue-ish color
        Gizmos.color = colliderGizmosColor;

        // Draw all colliders in the collider arrays
        if (drawColliderGizmos)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].DrawColliderGizmo();
            }
        }
    }
#endif
}