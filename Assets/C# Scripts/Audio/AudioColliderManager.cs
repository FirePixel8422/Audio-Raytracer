using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;



public class AudioColliderManager : MonoBehaviour
{
    private static List<AudioCollider> colliders;

    public static NativeListBatch<ColliderAABBStruct> AABBColliders { get; private set; }
    public static NativeListBatch<ColliderOBBStruct> OBBColliders { get; private set; }
    public static NativeListBatch<ColliderSphereStruct> SphereColliders { get; private set; }



    private void Awake()
    {
        // Fetch all AudioColliders in Scene
        colliders = this.FindObjectsOfType<AudioCollider>().ToList();
        int colliderCount = colliders.Count;

        AABBColliders = new NativeListBatch<ColliderAABBStruct>(colliderCount, Allocator.Persistent);
        OBBColliders = new NativeListBatch<ColliderOBBStruct>(colliderCount, Allocator.Persistent);
        SphereColliders = new NativeListBatch<ColliderSphereStruct>(colliderCount, Allocator.Persistent);

        DebugLogger.Throw("CRTICAL ERROR: The audio raytracer does NOT support more then more then 32767 audio colliders, audio system crashed", colliderCount > short.MaxValue);

        for (short i = 0; i < colliderCount; i++)
        {
            colliders[i].AddToAudioSystem(ref AABBColliders.NextBatch, ref OBBColliders.NextBatch, ref SphereColliders.NextBatch);
        }
    }

    public static void AddColiderToSystem(AudioCollider targetCollider)
    {
        targetCollider.AddToAudioSystem(ref AABBColliders.NextBatch, ref OBBColliders.NextBatch, ref SphereColliders.NextBatch);
        colliders.Add(targetCollider);
    }
    public static void RemoveColiderFromSystem(AudioCollider targetCollider)
    {
        colliders.RemoveAtSwapBack(targetCollider.AudioColliderId);
        short toRemoveId = targetCollider.AudioColliderId;

        switch (colliders[targetCollider.AudioColliderId].GetColliderType())
        {
            case ColliderType.Sphere:

                SphereColliders.RemoveAtSwapBack(toRemoveId);

                ColliderSphereStruct colliderSphereStruct = SphereColliders.NextBatch[toRemoveId];
                colliderSphereStruct.audioTargetId = toRemoveId;

                SphereColliders.NextBatch[toRemoveId] = colliderSphereStruct;
                break;

            case ColliderType.AABB:

                SphereColliders.RemoveAtSwapBack(toRemoveId);

                ColliderAABBStruct colliderAABBStruct = AABBColliders.NextBatch[toRemoveId];
                colliderAABBStruct.audioTargetId = targetCollider.AudioColliderId;

                AABBColliders.NextBatch[toRemoveId] = colliderAABBStruct;
                break;

            case ColliderType.OBB:

                OBBColliders.RemoveAtSwapBack(toRemoveId);

                ColliderOBBStruct colliderOBBStruct = OBBColliders.NextBatch[1];
                colliderOBBStruct.audioTargetId = toRemoveId;

                OBBColliders.NextBatch[toRemoveId] = colliderOBBStruct;
                break;

            default:
                DebugLogger.LogError("Null Collider detected");
                break;
        }
    }
    

    private void OnDestroy()
    {
        UpdateScheduler.CreateLateOnApplicationQuitCallback(Dispose);
    }

    private void Dispose()
    {
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