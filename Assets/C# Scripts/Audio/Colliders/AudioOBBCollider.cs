using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Include gameObject rotation to the colliders final rotation")]
    [SerializeField] private bool includeGameObjectRotation = true;

    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStruct = ColliderOBBStruct.Default;
    private ColliderOBBStruct lastColliderStruct;
    private Quaternion lastWorldRotation;


    public override ColliderType GetColliderType() => ColliderType.OBB;

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopy.size = scaledSize;

        AudioColliderId = (short)obbStructs.NextBatch.Length;
        obbStructs.Add(colliderStructCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        if (IgnoreScale == false)
        {
            Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopy.size = scaledSize;
        }

        obbStructs.Set(AudioColliderId, colliderStructCopy);
    }

    protected override void CheckColliderTransformation()
    {
        cachedTransform.GetPositionAndRotation(out Vector3 cWorldPosition, out Quaternion cWorldRotation);
        Vector3 cGlobalScale = IgnoreScale ? Vector3.zero : cachedTransform.lossyScale;

        if (cWorldPosition != lastWorldPosition ||
            cWorldRotation != lastWorldRotation ||
            (IgnoreScale == false && cGlobalScale != lastGlobalScale) ||
            colliderStruct != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);

            UpdateSavedData(cWorldPosition, cGlobalScale);
            lastWorldRotation = cWorldRotation;
        }
    }

    protected override void UpdateSavedData(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        base.UpdateSavedData(cWorldPosition, cGlobalScale);
        lastColliderStruct = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderOBBStruct colliderStructCopy = colliderStruct;

        if (includeGameObjectRotation)
        {
            colliderStructCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopy.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStructCopy.center, colliderStructCopy.Rotation, (float3)colliderStructCopy.size * 2);
    }
#endif
}