using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct ColliderAABBStruct
{
    public float3 center;
    public float3 size;

    [Header("How thick is this wall for permeation calculations")]
    public float thicknessMultiplier;

    [HideInInspector]
    public int audioTargetId;

    public static ColliderAABBStruct Default => new ColliderAABBStruct()
    {
        size = new float3(0.5f),
        thicknessMultiplier = 1,
        audioTargetId = -1,
    };
}