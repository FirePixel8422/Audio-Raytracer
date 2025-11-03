using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioAABBCollider : AudioCollider
{
    [Header("Box Colliders WITHOUT rotation: fast > 7/10")]
    [SerializeField] private ColliderAABBStruct colliderStruct = ColliderAABBStruct.Default;

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

        float3 mergedPosition = colliderStruct.center + (float3)transform.position;
        colliderStruct.center = mergedPosition;

        float3 scaledSize = colliderStruct.size * transform.lossyScale;
        colliderStruct.size = scaledSize;

        aabbStructs[cAABBId++] = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderAABBStruct colliderStruct = this.colliderStruct;

        float3 mergedPosition = colliderStruct.center + (float3)transform.position;
        colliderStruct.center = mergedPosition;

        float3 scaledSize = colliderStruct.size * transform.lossyScale;
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireCube(colliderStruct.center, colliderStruct.size * 2);
    }
#endif
}