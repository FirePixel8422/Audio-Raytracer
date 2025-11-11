using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;



public class AudioColliderManager : MonoBehaviour
{
    private static List<AudioCollider> colliders;

    [Header("Start capacity of collider arrays")]
    [SerializeField] private int startCapacity = 5;

    public static NativeListBatch<ColliderAABBStruct> AABBColliders { get; private set; }
    public static NativeListBatch<ColliderOBBStruct> OBBColliders { get; private set; }
    public static NativeListBatch<ColliderSphereStruct> SphereColliders { get; private set; }

    public static Action OnColliderUpdate { get; set; }



    
    private void Awake()
    {
        colliders = new List<AudioCollider>(startCapacity);

        AABBColliders = new NativeListBatch<ColliderAABBStruct>(startCapacity, Allocator.Persistent);
        OBBColliders = new NativeListBatch<ColliderOBBStruct>(startCapacity, Allocator.Persistent);
        SphereColliders = new NativeListBatch<ColliderSphereStruct>(startCapacity, Allocator.Persistent);
    }

    public static void AddColiderToSystem(AudioCollider targetCollider)
    {
        targetCollider.AddToAudioSystem(AABBColliders, OBBColliders, SphereColliders);
        colliders.Add(targetCollider);
    }
    public static void UpdateColiderInSystem(AudioCollider targetCollider)
    {
        targetCollider.UpdateToAudioSystem(AABBColliders, OBBColliders, SphereColliders);
    }
    public static void RemoveColiderFromSystem(AudioCollider targetCollider)
    {
        short toRemoveId = targetCollider.AudioColliderId;
        ColliderType colliderType = targetCollider.GetColliderType();

        // Remove collider from global list
        //colliders[^1].AudioColliderId = toRemoveId;
        colliders.RemoveSwapBack(targetCollider);

        switch (colliderType)
        {
            case ColliderType.Sphere:
                    SphereColliders.RemoveAtSwapBack(toRemoveId);
                    break;

            case ColliderType.AABB:
                    AABBColliders.RemoveAtSwapBack(toRemoveId);
                    break;

            case ColliderType.OBB:
                    OBBColliders.RemoveAtSwapBack(toRemoveId);
                    break;

            default:
                DebugLogger.LogError("Null Collider detected");
                break;
        }
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