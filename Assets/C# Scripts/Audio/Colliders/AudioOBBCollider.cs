using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStruct = ColliderOBBStruct.Default;

    [Header("Include gameObject rotation shorto the colliders final rotation")]
    [SerializeField] private bool includeGameObjectRotation = true;


    public override ColliderType GetColliderType()
    {
        return ColliderType.OBB;
    }

    public override void AddToAudioSystem(
        ref NativeArray<ColliderAABBStruct> aabbStructs, ref short cAABBId,
        ref NativeArray<ColliderOBBStruct> obbStructs, ref short cOBBId,
        ref NativeArray<ColliderSphereStruct> sphereStructs, ref short cSphereId,
        short audioColliderId)
    {
        base.AddToAudioSystem(ref aabbStructs, ref cAABBId, ref obbStructs, ref cOBBId, ref sphereStructs, ref cSphereId, audioColliderId);

        var colliderStruct = this.colliderStruct;

        if (TryGetComponent(out AudioTargetRT rtTarget))
        {
            colliderStruct.audioTargetId = rtTarget.Id;
        }

        if (includeGameObjectRotation)
        {
            colliderStruct.Rotation *= transform.rotation;
        }

        Half3.Add(transform.rotation * colliderStruct.center.ToFloat3(), transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        obbStructs[cOBBId++] = colliderStruct;
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