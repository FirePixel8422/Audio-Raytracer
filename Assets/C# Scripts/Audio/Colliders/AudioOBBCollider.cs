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
        ColliderOBBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.AudioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopy.Center, transform.position, out half3 mergedPosition);
        colliderStructCopy.Center = mergedPosition;

        if (IgnoreScale)
        {
            Half3.Multiply(colliderStructCopy.Size, lastGlobalScale, out half3 scaledSize);
            colliderStructCopy.Size = scaledSize;
        }
        else
        {
            Half3.Multiply(colliderStructCopy.Size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopy.Size = scaledSize;
        }

        // Invert rotation for audio system calculation optimization later
        colliderStructCopy.Rotation = math.inverse(colliderStructCopy.Rotation);

        AudioColliderId = (short)obbStructs.NextBatch.Length;
        obbStructs.Add(colliderStructCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        ColliderOBBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.AudioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStructCopy.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStructCopy.Center, transform.position, out half3 mergedPosition);
        colliderStructCopy.Center = mergedPosition;

        if (IgnoreScale)
        {
            Half3.Multiply(colliderStructCopy.Size, lastGlobalScale, out half3 scaledSize);
            colliderStructCopy.Size = scaledSize;
        }
        else
        {
            Half3.Multiply(colliderStructCopy.Size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopy.Size = scaledSize;
        }

        // Invert rotation for audio system calculation optimization later
        colliderStructCopy.Rotation = math.inverse(colliderStructCopy.Rotation);

        obbStructs[AudioColliderId] = colliderStructCopy;
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

            UpdateSavedTransformation(cWorldPosition, cGlobalScale);
            lastWorldRotation = cWorldRotation;
        }
    }

    protected override void UpdateSavedTransformation(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        base.UpdateSavedTransformation(cWorldPosition, cGlobalScale);
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
        
        Half3.Add(transform.rotation * (float3)colliderStructCopy.Center, transform.position, out half3 mergedPosition);
        colliderStructCopy.Center = mergedPosition;

        Half3.Multiply(colliderStructCopy.Size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopy.Size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStructCopy.Center, colliderStructCopy.Rotation, (float3)colliderStructCopy.Size * 2);
    }
#endif
}