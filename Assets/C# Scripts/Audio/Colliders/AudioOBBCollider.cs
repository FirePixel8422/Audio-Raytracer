using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioOBBCollider : AudioCollider
{
    [Header("Box Colliders with rotation: \nfast, but a little slower than an 'axisAlignedBox' > 6/10")]
    [SerializeField] private ColliderOBBStruct colliderStruct = ColliderOBBStruct.Default;

    [Header("Include gameObject rotation into the colliders final rotation")]
    [SerializeField] private bool includeGameObjectRotation = true;


    public override void AddToAudioSystem(
        ref NativeArray<ColliderAABBStruct> aabbStructs, ref int cAABBId,
        ref NativeArray<ColliderOBBStruct> obbStructs, ref int cOBBId,
        ref NativeArray<ColliderSphereStruct> sphereStructs, ref int cSphereId,
        int audioColliderId)
    {
        base.AddToAudioSystem(ref aabbStructs, ref cAABBId, ref obbStructs, ref cOBBId, ref sphereStructs, ref cSphereId, audioColliderId);

        var colliderStruct = this.colliderStruct;

        if (TryGetComponent(out AudioTargetRT rtTarget))
        {
            colliderStruct.audioTargetId = rtTarget.id;
        }

        if (includeGameObjectRotation)
        {
            colliderStruct.rotation *= transform.rotation;
        }

        float3 mergedPosition = colliderStruct.center + (float3)transform.position;
        colliderStruct.center = mergedPosition;

        float3 scaledSize = colliderStruct.size * transform.lossyScale;
        colliderStruct.size = scaledSize;

        obbStructs[cOBBId++] = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderOBBStruct colliderStruct = this.colliderStruct;

        if (includeGameObjectRotation)
        {
            colliderStruct.rotation *= transform.rotation;
        }

        float3 mergedPosition = colliderStruct.center + (float3)transform.position;
        colliderStruct.center = mergedPosition;

        float3 scaledSize = colliderStruct.size * transform.lossyScale;
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, colliderStruct.center, colliderStruct.rotation, colliderStruct.size * 2);
    }
#endif
}