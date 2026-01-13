using Unity.Mathematics;
using UnityEngine;


public class AudioAABBCollider : AudioCollider
{
    [Header("Box Colliders WITHOUT rotation: fast > 7/10")]
    [SerializeField] private ColliderAABBStruct colliderStructCopy = ColliderAABBStruct.Default;
    private ColliderAABBStruct lastColliderStruct;


    public override ColliderType GetColliderType() => ColliderType.AABB;

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStructCopyCopy = colliderStructCopy;

        colliderStructCopyCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopyCopy.size = scaledSize;

        AudioColliderId = (short)aabbStructs.NextBatch.Length;
        aabbStructs.Add(colliderStructCopyCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStructCopyCopy = colliderStructCopy;

        colliderStructCopyCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        if (IgnoreScale == false)
        {
            Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopyCopy.size = scaledSize;
        }

        aabbStructs.Set(AudioColliderId, colliderStructCopyCopy);
    }

    protected override void CheckColliderTransformation()
    {
        Vector3 cWorldPosition = cachedTransform.position;
        Vector3 cGlobalScale = IgnoreScale ? Vector3.zero : cachedTransform.lossyScale;

        if (cWorldPosition != lastWorldPosition ||
            (IgnoreScale == false && cGlobalScale != lastGlobalScale) ||
            colliderStructCopy != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);

            UpdateSavedData(cWorldPosition, cGlobalScale);
        }
    }

    protected override void UpdateSavedData(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        base.UpdateSavedData(cWorldPosition, cGlobalScale);
        lastColliderStruct = colliderStructCopy;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderAABBStruct colliderStructCopyCopy = colliderStructCopy;

        Half3.Add(colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopyCopy.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStructCopyCopy.center, Quaternion.identity, (float3)colliderStructCopyCopy.size * 2);
    }
#endif
}