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

    public override void AddToAudioSystem(ref NativeList<ColliderAABBStruct> aabbStructs, ref NativeList<ColliderOBBStruct> obbStructs, ref NativeList<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(ref aabbStructs, ref obbStructs, ref sphereStructs);

        ColliderOBBStruct colliderStruct = this.colliderStruct;

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

        AudioColliderId = (short)obbStructs.Length;
        obbStructs.Add(colliderStruct);
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