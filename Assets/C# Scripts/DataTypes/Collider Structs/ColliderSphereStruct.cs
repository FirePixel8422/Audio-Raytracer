using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[Serializable]
public struct ColliderSphereStruct
{
    public half3 center;
    public half radius;

    [Header("How thick is this wall for permeation calculations")]
    public half thicknessMultiplier;
    [Header("How much power of the audioRays hitting this surface gets consumed")]
    public half absorptionValue;

    public short audioTargetId;

    public static ColliderSphereStruct Default => new ColliderSphereStruct()
    {
        radius = (half)0.5f,
        thicknessMultiplier = (half)1,
        audioTargetId = -1,
    };

    public static bool operator ==(ColliderSphereStruct a, ColliderSphereStruct b)
    {
        return a.center.x.value == b.center.x.value &&
               a.center.y.value == b.center.y.value &&
               a.center.z.value == b.center.z.value &&
               a.radius.value == b.radius.value &&
               a.thicknessMultiplier == b.thicknessMultiplier &&
               a.audioTargetId == b.audioTargetId;
    }

    public static bool operator !=(ColliderSphereStruct a, ColliderSphereStruct b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is ColliderSphereStruct other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(center, radius, thicknessMultiplier);
    }
}