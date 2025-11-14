using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;


public class AudioColliderManager : MonoBehaviour
{
    private static List<AudioCollider> colliders;

    private static List<AudioCollider> sphereColliderRefs;
    private static List<AudioCollider> aabbColliderRefs;
    private static List<AudioCollider> obbColliderRefs;

    [Header("Start capacity of collider arrays")]
    [SerializeField] private int startCapacity = 5;

    public static NativeListBatch<ColliderAABBStruct> AABBColliders { get; private set; }
    public static NativeListBatch<ColliderOBBStruct> OBBColliders { get; private set; }
    public static NativeListBatch<ColliderSphereStruct> SphereColliders { get; private set; }

    public static Action OnColliderUpdate { get; set; }

    private void Awake()
    {
        colliders = new List<AudioCollider>(startCapacity);

        sphereColliderRefs = new List<AudioCollider>(startCapacity);
        aabbColliderRefs = new List<AudioCollider>(startCapacity);
        obbColliderRefs = new List<AudioCollider>(startCapacity);

        AABBColliders = new NativeListBatch<ColliderAABBStruct>(startCapacity, Allocator.Persistent);
        OBBColliders = new NativeListBatch<ColliderOBBStruct>(startCapacity, Allocator.Persistent);
        SphereColliders = new NativeListBatch<ColliderSphereStruct>(startCapacity, Allocator.Persistent);
    }

    public static void AddColiderToSystem(AudioCollider target)
    {
        target.AddToAudioSystem(AABBColliders, OBBColliders, SphereColliders);
        colliders.Add(target);

        // Add to type-specific ref list
        switch (target.GetColliderType())
        {
            case ColliderType.Sphere:
                sphereColliderRefs.Add(target);
                break;

            case ColliderType.AABB:
                aabbColliderRefs.Add(target);
                break;

            case ColliderType.OBB:
                obbColliderRefs.Add(target);
                break;
        }
    }

    public static void RemoveColiderFromSystem(AudioCollider target)
    {
        if (target == null) return;

        short removeId = target.AudioColliderId;
        ColliderType type = target.GetColliderType();

        // Remove from master list
        colliders.RemoveSwapBack(target);

        // Remove from type-specific list and native list in O(1)
        switch (type)
        {
            case ColliderType.Sphere:
                SwapRemove(SphereColliders, sphereColliderRefs, removeId);
                break;

            case ColliderType.AABB:
                SwapRemove(AABBColliders, aabbColliderRefs, removeId);
                break;

            case ColliderType.OBB:
                SwapRemove(OBBColliders, obbColliderRefs, removeId);
                break;
        }
    }

    private static void SwapRemove<T>(NativeListBatch<T> nativeList, List<AudioCollider> refList, short removeId) where T : unmanaged
    {
        if (removeId < 0 || removeId >= refList.Count || removeId >= nativeList.NextBatch.Length)
            return; // skip invalid remove

        int lastIndex = refList.Count - 1;

        if (removeId != lastIndex)
        {
            refList[removeId] = refList[lastIndex];
            refList[removeId].AudioColliderId = removeId;
        }

        refList.RemoveAt(lastIndex);
        nativeList.RemoveAtSwapBack(removeId);
    }


    public static void UpdateColiderInSystem(AudioCollider targetCollider)
    {
        targetCollider.UpdateToAudioSystem(AABBColliders, OBBColliders, SphereColliders);
    }

    public static void CycleToNextBatch()
    {
        OnColliderUpdate?.Invoke();

        AABBColliders.CycleToNextBatch();
        OBBColliders.CycleToNextBatch();
        SphereColliders.CycleToNextBatch();
    }

    private void OnDestroy()
    {
        UpdateScheduler.CreateLateOnApplicationQuitCallback(Dispose);
    }

    private void Dispose()
    {
        OnColliderUpdate = null;

        AABBColliders.Dispose();
        OBBColliders.Dispose();
        SphereColliders.Dispose();
    }

#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private bool drawColliderGizmos = true;
    [SerializeField] private Color colliderGizmosColor = new Color(1f, 0.75f, 0.25f);
    public readonly static Color ColliderGizmosSelectedColor = new Color(1f, 0.75f, 0.25f);

    private void OnDrawGizmos()
    {
        AudioCollider[] colliders = this.FindObjectsOfType<AudioCollider>(false);

        Gizmos.color = colliderGizmosColor;

        if (drawColliderGizmos)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].DrawColliderGizmo();
            }
        }
    }

    private void Update()
    {
        DEBUG_AABBColliders = AABBColliders;
        DEBUG_OBBColliders = OBBColliders;
        DEBUG_SphereColliders = SphereColliders;
    }

    public NativeListBatch<ColliderAABBStruct> DEBUG_AABBColliders;
    public NativeListBatch<ColliderOBBStruct> DEBUG_OBBColliders;
    public NativeListBatch<ColliderSphereStruct> DEBUG_SphereColliders;
#endif

}
