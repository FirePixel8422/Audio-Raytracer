using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioAABBCollider : AudioCollider
{
    [Header("Box Colliders WITHOUT rotation: fast > 7/10")]
    [SerializeField] private ColliderAABBStruct colliderStruct = ColliderAABBStruct.Default;


    public override ColliderType GetColliderType()
    {
        return ColliderType.AABB;
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

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition); 
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        aabbStructs[cAABBId++] = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderAABBStruct colliderStruct = this.colliderStruct;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, colliderStruct.center.ToFloat3(), Quaternion.identity, colliderStruct.size.ToFloat3() * 2);
    }
#endif
}