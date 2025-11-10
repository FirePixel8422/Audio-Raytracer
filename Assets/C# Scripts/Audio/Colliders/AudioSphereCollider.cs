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

    public override void AddToAudioSystem(ref NativeList<ColliderAABBStruct> aabbStructs, ref NativeList<ColliderOBBStruct> obbStructs, ref NativeList<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(ref aabbStructs, ref obbStructs, ref sphereStructs);

        ColliderSphereStruct colliderStruct = this.colliderStruct;

        if (TryGetComponent(out AudioTargetRT rtTarget))
        {
            colliderStruct.audioTargetId = rtTarget.Id;
        }

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half.Multiply(colliderStruct.radius, math.max(transform.lossyScale.x, math.max(transform.lossyScale.y, transform.lossyScale.z)), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        AudioColliderId = (short)sphereStructs.Length;
        sphereStructs.Add(colliderStruct);
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