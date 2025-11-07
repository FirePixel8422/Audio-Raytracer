using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ColliderAABBStruct
{
    public half3 center;
    public half3 size;

    [Header("How thick is this wall for permeation calculations")]
    public half thicknessMultiplier;

    [HideInInspector]
    public short audioTargetId;

    public static ColliderAABBStruct Default => new ColliderAABBStruct()
    {
        size = new half3(0.5f),
        thicknessMultiplier = (half)1,
        audioTargetId = -1,
    };
}