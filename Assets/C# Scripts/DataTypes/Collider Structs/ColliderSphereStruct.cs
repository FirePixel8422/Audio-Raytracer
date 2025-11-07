using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct ColliderSphereStruct
{
    public half3 center;
    public half radius;

    [Header("How thick is this wall for permeation calculations")]
    public half thicknessMultiplier;

    [HideInInspector]
    public short audioTargetId;

    public static ColliderSphereStruct Default => new ColliderSphereStruct()
    {
        radius = (half)0.5f,
        thicknessMultiplier = (half)1,
        audioTargetId = -1,
    };
}