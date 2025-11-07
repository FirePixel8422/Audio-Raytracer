using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioSphereCollider : AudioCollider
{
    [Header("Sphere Collider: very fast > 10/10")]
    [SerializeField] private ColliderSphereStruct colliderStruct = ColliderSphereStruct.Default;


    public override ColliderType GetColliderType()
    {
        return ColliderType.Sphere;
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

        Half.Multiply(colliderStruct.radius, math.max(transform.lossyScale.x, math.max(transform.lossyScale.y, transform.lossyScale.z)), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        sphereStructs[cSphereId++] = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderSphereStruct colliderStruct = this.colliderStruct;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half.Multiply(colliderStruct.radius, math.max(transform.lossyScale.x, math.max(transform.lossyScale.y, transform.lossyScale.z)), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        Gizmos.DrawWireSphere(colliderStruct.center.ToFloat3(), (float)colliderStruct.radius);
    }
#endif
}