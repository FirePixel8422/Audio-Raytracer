using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStructCopy = ColliderOBBStruct.Default;
    private ColliderOBBStruct lastColliderStruct;
    private Quaternion lastWorldRotation;

    [Header("Include gameObject rotation to the colliders final rotation")]
    [SerializeField] private bool includeGameObjectRotation = true;


    public override ColliderType GetColliderType()
    {
        return ColliderType.OBB;
    }

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStructCopyCopy = colliderStructCopy;

        colliderStructCopyCopy.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopyCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopyCopy.size = scaledSize;

        AudioColliderId = (short)obbStructs.NextBatch.Length;
        obbStructs.Add(colliderStructCopyCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStructCopyCopy = colliderStructCopy;

        colliderStructCopyCopy.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopyCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        if (IgnoreScale == false)
        {
            Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopyCopy.size = scaledSize;
        }

        obbStructs.Set(AudioColliderId, colliderStructCopyCopy);
    }

    protected override void CheckColliderTransformation()
    {
        cachedTransform.GetPositionAndRotation(out Vector3 cWorldPosition, out Quaternion cWorldRotation);
        Vector3 cGlobalScale = IgnoreScale ? Vector3.zero : cachedTransform.lossyScale;

        if (cWorldPosition != lastWorldPosition ||
            cWorldRotation != lastWorldRotation ||
            (IgnoreScale == false && cGlobalScale != lastGlobalScale) ||
            colliderStructCopy != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);

            UpdateSavedData(cWorldPosition, cGlobalScale);
            lastWorldRotation = cWorldRotation;
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
        ColliderOBBStruct colliderStructCopyCopy = colliderStructCopy;

        if (includeGameObjectRotation)
        {
            colliderStructCopyCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopyCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopyCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopyCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopyCopy.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStructCopyCopy.center, colliderStructCopyCopy.Rotation, (float3)colliderStructCopyCopy.size * 2);
    }
#endif
}