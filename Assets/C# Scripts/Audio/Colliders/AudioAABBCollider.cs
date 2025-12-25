using Unity.Mathematics;
using UnityEngine;


public class AudioAABBCollider : AudioCollider
{
    [Header("Box Colliders WITHOUT rotation: fast > 7/10")]
    [SerializeField] private ColliderAABBStruct colliderStruct = ColliderAABBStruct.Default;
    private ColliderAABBStruct lastColliderStruct;


    public override ColliderType GetColliderType()
    {
        return ColliderType.AABB;
    }

    public override void AddToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition); 
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        AudioColliderId = (short)aabbStructs.NextBatch.Length;
        aabbStructs.Add(colliderStruct);
    }

    public override void UpdateToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs)
    {
        base.AddToAudioSystem(aabbStructs, obbStructs, sphereStructs);

        ColliderAABBStruct colliderStruct = this.colliderStruct;

        colliderStruct.audioTargetId = AudioTargetId;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition); 
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        aabbStructs.Set(AudioColliderId, colliderStruct);
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
        ColliderAABBStruct colliderStruct = this.colliderStruct;

        Half3.Add(colliderStruct.center, transform.position, out half3 mergedPosition);
        colliderStruct.center = mergedPosition;

        Half3.Multiply(colliderStruct.size, transform.lossyScale, out half3 scaledSize);
        colliderStruct.size = scaledSize;

        Gizmos.DrawWireMesh(GlobalMeshes.cube, (float3)colliderStruct.center, Quaternion.identity, (float3)colliderStruct.size * 2);
    }
#endif
}