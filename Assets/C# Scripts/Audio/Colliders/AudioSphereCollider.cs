using Unity.Mathematics;
using UnityEngine;


public class AudioSphereCollider : AudioCollider
{
    [Header("Sphere Collider: very fast > 10/10")]
    [SerializeField] private ColliderSphereStruct colliderStruct = ColliderSphereStruct.Default;
    private ColliderSphereStruct lastColliderStruct;


    public override ColliderType GetColliderType()
    {
        return ColliderType.Sphere;
    }

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderSphereStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half.Multiply(colliderStruct.radius, GetLargestPositionComponent(transform.lossyScale), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        AudioColliderId = (short)sphereStructs.NextBatch.Length;
        sphereStructs.Add(colliderStruct);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderSphereStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half.Multiply(colliderStruct.radius, GetLargestPositionComponent(transform.lossyScale), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        sphereStructs.Set(AudioColliderId, colliderStruct);
    }

    private half GetLargestPositionComponent(Vector3 input)
    {
        return (half)math.max(input.x, math.max(input.y, input.z));
    }

    protected override void CheckColliderTransformation()
    {
        Vector3 cWorldPosition = cachedTransform.position;
        Vector3 cGlobalScale = IgnoreScale ? Vector3.zero : cachedTransform.lossyScale;

        if (cWorldPosition != lastWorldPosition ||
            (IgnoreScale == false && cGlobalScale != lastGlobalScale) ||
            colliderStruct != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);

            UpdateSavedData(cWorldPosition, cGlobalScale);
        }
    }

    protected override void UpdateSavedData(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        base.UpdateSavedData(cWorldPosition, cGlobalScale);
        lastColliderStruct = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderSphereStruct colliderStruct = this.colliderStruct;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half.Multiply(colliderStruct.radius, GetLargestPositionComponent(transform.lossyScale), out half scaledRadius);
        colliderStruct.radius = scaledRadius;

        Gizmos.DrawWireSphere((float3)colliderStruct.center, (float)colliderStruct.radius);
    }
#endif
}