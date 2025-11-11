using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStruct = ColliderOBBStruct.Default;
    private ColliderOBBStruct lastColliderStruct;

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

        Half3.Add(transform.rotation * colliderStruct.center.ToFloat3(), transform.position, out half3 mergedPosition);
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

        Half3.Add(transform.rotation * colliderStruct.center.ToFloat3(), transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        obbStructs.Set(AudioColliderId, colliderStruct);
    }

    protected override void CheckColliderTransformation()
    {
        if (transform.position != lastWorldPosition)
        {
            AudioColliderManager.UpdateColiderInSystem(this);
        }
        else if (transform.lossyScale != lastGlobalScale)
        {
            AudioColliderManager.UpdateColiderInSystem(this);
        }
        else if (colliderStruct != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);
        }
        UpdateSavedData();
    }

    protected override void UpdateSavedData()
    {
        base.UpdateSavedData();
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

        Half3.Add(transform.rotation * colliderStruct.center.ToFloat3(), transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, colliderStruct.center.ToFloat3(), colliderStruct.Rotation, colliderStruct.size.ToFloat3() * 2);
    }
#endif
}