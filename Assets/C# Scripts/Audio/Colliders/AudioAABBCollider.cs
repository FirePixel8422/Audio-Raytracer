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
        if (transform.position != lastWorldPosition ||
            transform.lossyScale != lastGlobalScale ||
            colliderStruct != lastColliderStruct)
        {
            AudioColliderManager.UpdateColiderInSystem(this);
        }
        UpdateSavedData();
    }

    protected override void UpdateSavedData()
    {
        base.UpdateSavedData();
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