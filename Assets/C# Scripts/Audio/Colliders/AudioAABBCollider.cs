using System.Linq;
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

    public override void AddToAudioSystem(ref NativeList<ColliderAABBStruct> aabbStructs, ref NativeList<ColliderOBBStruct> obbStructs, ref NativeList<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(ref aabbStructs, ref obbStructs, ref sphereStructs);

        ColliderAABBStruct colliderStruct = this.colliderStruct;

        if (TryGetComponent(out AudioTargetRT rtTarget))
        {
            colliderStruct.audioTargetId = rtTarget.Id;
        }

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition); 
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        AudioColliderId = (short)aabbStructs.Length;
        aabbStructs.Add(colliderStruct);
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