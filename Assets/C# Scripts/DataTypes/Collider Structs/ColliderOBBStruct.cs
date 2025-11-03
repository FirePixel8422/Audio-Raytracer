using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct ColliderOBBStruct
{
    public float3 center;
    public float3 size;

    public quaternion rotation;

    [Header("How thick is this wall for permeation calculations")]
    public float thicknessMultiplier;

    [HideInInspector]
    public int audioTargetId;


    public static ColliderOBBStruct Default => new ColliderOBBStruct()
    {
        size = new float3(0.5f),
        rotation = quaternion.identity,
        thicknessMultiplier = 1,
        audioTargetId = -1,
    };
}