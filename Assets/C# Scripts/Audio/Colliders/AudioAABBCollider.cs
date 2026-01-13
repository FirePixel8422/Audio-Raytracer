using Unity.Mathematics;
using UnityEngine;


public class AudioAABBCollider : AudioCollider
{
    [Header("Box Colliders WITHOUT rotation: fast > 7/10")]
    [SerializeField] private ColliderAABBStruct colliderStruct = ColliderAABBStruct.Default;
    private ColliderAABBStruct lastColliderStruct;


    public override ColliderType GetColliderType() => ColliderType.AABB;

    public override void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopy.size = scaledSize;

        AudioColliderId = (short)aabbStructs.NextBatch.Length;
        aabbStructs.Add(colliderStructCopy);
    }

    public override void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStructCopy = colliderStruct;

        colliderStructCopy.audioTargetId = AudioTargetId;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        if (IgnoreScale == false)
        {
            Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
            colliderStructCopy.size = scaledSize;
        }

        aabbStructs.Set(AudioColliderId, colliderStructCopy);
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
        ColliderAABBStruct colliderStructCopy = colliderStruct;

        Half3.Add(colliderStructCopy.center, transform.position, out half3 mergedPosition);
        colliderStructCopy.center = mergedPosition;

        Half3.Multiply(colliderStructCopy.size, transform.lossyScale, out half3 scaledSize);
        colliderStructCopy.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStructCopy.center, Quaternion.identity, (float3)colliderStructCopy.size * 2);
    }
#endif
}