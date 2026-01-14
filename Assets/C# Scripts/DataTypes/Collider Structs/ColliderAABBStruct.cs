using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[Serializable]
public struct ColliderAABBStruct
{
    public half3 center;
    public half3 size;

    [Header("How thick is this wall for permeation calculations")]
    public half thicknessMultiplier;
    [Header("How much power of the audioRays hitting this surface gets consumed")]
    public half absorptionValue;

    public short audioTargetId;

    public static ColliderAABBStruct Default => new ColliderAABBStruct()
    {
        size = new half3(0.5f),
        thicknessMultiplier = (half)1,
        audioTargetId = -1,
    };

    public static bool operator ==(ColliderAABBStruct a, ColliderAABBStruct b)
    {
        return a.center.x.value == b.center.x.value &&
               a.center.y.value == b.center.y.value &&
               a.center.z.value == b.center.z.value &&
               a.size.x.value == b.size.x.value &&
               a.size.y.value == b.size.y.value &&
               a.size.z.value == b.size.z.value &&
               a.thicknessMultiplier.value == b.thicknessMultiplier.value &&
               a.absorptionValue.value == b.absorptionValue.value &&
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