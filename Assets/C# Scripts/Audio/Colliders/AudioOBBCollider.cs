using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStruct = ColliderOBBStruct.Default;
    private ColliderOBBStruct lastColliderStruct;
    private Quaternion lastWorldRotation;

    [Header("Include gameObject rotation to the colliders final rotation")]
    [SerializeField] private bool includeGameObjectRotation = true;


    public override ColliderType GetColliderType()
    {
        return ColliderType.OBB;
    }

    public override void AddToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStruct.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        AudioColliderId = (short)obbStructs.NextBatch.Length;
        obbStructs.Add(colliderStruct);
    }

    public override void UpdateToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderOBBStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        if (includeGameObjectRotation)
        {
            colliderStruct.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        obbStructs.Set(AudioColliderId, colliderStruct);
    }

    protected override void CheckColliderTransformation()
    {
        if (transform.position != lastWorldPosition ||
            transform.lossyScale != lastGlobalScale ||
            transform.rotation != lastWorldRotation ||
            colliderStruct != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);
        }
        UpdateSavedData();
    }

    protected override void UpdateSavedData()
    {
        base.UpdateSavedData();
        lastWorldRotation = transform.rotation;
        lastColliderStruct = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderOBBStruct colliderStruct = this.colliderStruct;

        if (includeGameObjectRotation)
        {
            colliderStruct.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * (float3)colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStruct.center, colliderStruct.Rotation, (float3)colliderStruct.size * 2);
    }
#endif
}