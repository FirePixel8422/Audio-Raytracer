using System;
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

    public static bool operator ==(ColliderAABBStruct a, ColliderAABBStruct b)
    {
        return math.all(a.center == b.center) &&
               math.all(a.size == b.size) &&
               a.thicknessMultiplier == b.thicknessMultiplier &&
               a.audioTargetId == b.audioTargetId;
    }

    public static bool operator !=(ColliderAABBStruct a, ColliderAABBStruct b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is ColliderAABBStruct other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(center, size, thicknessMultiplier);
    }
}