using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;


public class AudioColliderManager : MonoBehaviour
{
    [Header("Start capacity of collider arrays")]
    [SerializeField] private int startCapacity = 5;

    private static List<AudioCollider> colliders;

    private static List<AudioCollider> sphereColliderRefs;
    private static List<AudioCollider> aabbColliderRefs;
    private static List<AudioCollider> obbColliderRefs;

    public static NativeJobBatch<ColliderAABBStruct> AABBColliders { get; private set; }
    public static NativeJobBatch<ColliderOBBStruct> OBBColliders { get; private set; }
    public static NativeJobBatch<ColliderSphereStruct> SphereColliders { get; private set; }

    public static Action OnColliderUpdate { get; set; }


    private void Awake()
    {
        colliders = new List<AudioCollider>(startCapacity);

        sphereColliderRefs = new List<AudioCollider>(startCapacity);
        aabbColliderRefs = new List<AudioCollider>(startCapacity);
        obbColliderRefs = new List<AudioCollider>(startCapacity);

        AABBColliders = new NativeJobBatch<ColliderAABBStruct>(startCapacity, Allocator.Persistent);
        OBBColliders = new NativeJobBatch<ColliderOBBStruct>(startCapacity, Allocator.Persistent);
        SphereColliders = new NativeJobBatch<ColliderSphereStruct>(startCapacity, Allocator.Persistent);
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

        short toRemoveId = target.AudioColliderId;
        ColliderType type = target.GetColliderType();

        // Remove from master list
        colliders.RemoveSwapBack(target);

        // Remove from type-specific list and native list in O(1)
        switch (type)
        {
            case ColliderType.Sphere:
                SwapRemove(SphereColliders, sphereColliderRefs, toRemoveId);
                break;

            case ColliderType.AABB:
                SwapRemove(AABBColliders, aabbColliderRefs, toRemoveId);
                break;

            case ColliderType.OBB:
                SwapRemove(OBBColliders, obbColliderRefs, toRemoveId);
                break;
        }
    }

    private static void SwapRemove<T>(NativeJobBatch<T> nativeList, List<AudioCollider> refList, short toRemoveId) where T : unmanaged
    {
        if (toRemoveId < 0 || toRemoveId >= refList.Count || toRemoveId >= nativeList.NextBatch.Length)
            return; // skip invalid remove

        int lastIndex = refList.Count - 1;

        if (toRemoveId != lastIndex)
        {
            refList[toRemoveId] = refList[lastIndex];
            refList[toRemoveId].AudioColliderId = toRemoveId;
        }

        refList.RemoveAt(lastIndex);
        nativeList.RemoveAtSwapBack(toRemoveId);
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

    public NativeJobBatch<ColliderAABBStruct> DEBUG_AABBColliders;
    public NativeJobBatch<ColliderOBBStruct> DEBUG_OBBColliders;
    public NativeJobBatch<ColliderSphereStruct> DEBUG_SphereColliders;
#endif

}
