using Unity.Mathematics;
using UnityEngine;


public class AudioSphereCollider : AudioCollider
{
    [Header("Sphere Collider: very fast > 10/10")]
    [SerializeField] private ColliderSphereStruct colliderStruct = ColliderSphereStruct.Default;
    private ColliderSphereStruct lastColliderStruct;


    public override ColliderType GetColliderType() => ColliderType.Sphere;

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        ColliderSphereStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        if (IgnoreScale)
        {
            Half.Multiply(colliderStructCopy.radius, GetLargestComponent(lastGlobalScale), out half scaledRadius);
            colliderStructCopy.radius = scaledRadius;
        }
        else
        {
            Half.Multiply(colliderStructCopy.radius, GetLargestComponent(transform.lossyScale), out half scaledRadius);
            colliderStructCopy.radius = scaledRadius;
        }

        AudioColliderId = (short)sphereStructs.NextBatch.Length;
        sphereStructs.Add(colliderStructCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        ColliderSphereStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        if (IgnoreScale)
        {
            Half.Multiply(colliderStructCopy.radius, GetLargestComponent(lastGlobalScale), out half scaledRadius);
            colliderStructCopy.radius = scaledRadius;
        }
        else
        {
            Half.Multiply(colliderStructCopy.radius, GetLargestComponent(transform.lossyScale), out half scaledRadius);
            colliderStructCopy.radius = scaledRadius;
        }

        sphereStructs.Set(AudioColliderId, colliderStructCopy);
    }

    private half GetLargestComponent(Vector3 input)
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

            UpdateSavedTransformation(cWorldPosition, cGlobalScale);
        }
    }

    protected override void UpdateSavedTransformation(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        base.UpdateSavedTransformation(cWorldPosition, cGlobalScale);
        lastColliderStruct = colliderStruct;
    }


#if UNITY_EDITOR
    public override void DrawColliderGizmo()
    {
        ColliderSphereStruct colliderStructCopy = colliderStruct;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        Half.Multiply(colliderStructCopy.radius, GetLargestComponent(transform.lossyScale), out half scaledRadius);
        colliderStructCopy.radius = scaledRadius;

        Gizmos.DrawWireSphere((float3)colliderStructCopy.center, (float)colliderStructCopy.radius);
    }
#endif
}