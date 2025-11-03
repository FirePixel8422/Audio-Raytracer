using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct ColliderSphereStruct
{
    public float3 center;
    public float radius;

    [Header("How thick is this wall for permeation calculations")]
    public float thicknessMultiplier;

    [HideInInspector]
    public int audioTargetId;

    public static ColliderSphereStruct Default => new ColliderSphereStruct()
    {
        radius = 0.5f,
        thicknessMultiplier = 1,
        audioTargetId = -1,
    };
}