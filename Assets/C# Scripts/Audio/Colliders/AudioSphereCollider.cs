using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioSphereCollider : AudioCollider
{
    [Header("Sphere Collider: very fast > 10/10")]
    [SerializeField] private ColliderSphereStruct colliderStruct = ColliderSphereStruct.Default;

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

        float scaledRadius = colliderStruct.radius * math.max(transform.lossyScale.x, math.max(transform.lossyScale.y, transform.lossyScale.z));
        colliderStruct.radius = scaledRadius;

        sphereStructs[cSphereId++] = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderSphereStruct colliderStruct = this.colliderStruct;

        float3 mergedPosition = colliderStruct.center + (float3)transform.position;
        colliderStruct.center = mergedPosition;

        float scaledRadius = colliderStruct.radius * math.max(transform.lossyScale.x, math.max(transform.lossyScale.y, transform.lossyScale.z));
        colliderStruct.radius = scaledRadius;

        Gizmos.DrawWireSphere(colliderStruct.center, colliderStruct.radius);
    }
#endif
}