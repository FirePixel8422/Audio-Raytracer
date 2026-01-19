using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[Serializable]
public struct ColliderSphereStruct
{
    public half3 Center;
    public half Radius;

    [Header("How much power gets consumed when permeation rays go through Material")]
    public half MaterialDensity;

    [Header("How much power of the audioRays hitting this surface gets consumed")]
    public half MaterialAbsorption;

    public short AudioTargetId;

    public static ColliderSphereStruct Default => new ColliderSphereStruct()
    {
        Radius = (half)0.5f,
        AudioTargetId = -1,
    };

    public static bool operator ==(ColliderSphereStruct a, ColliderSphereStruct b)
    {
        return a.Center.x.value == b.Center.x.value &&
               a.Center.y.value == b.Center.y.value &&
               a.Center.z.value == b.Center.z.value &&
               a.Radius.value == b.Radius.value &&
               a.MaterialDensity.value == b.MaterialDensity.value &&
               a.MaterialAbsorption.value == b.MaterialAbsorption.value &&
               a.AudioTargetId == b.AudioTargetId;
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
        return HashCode.Combine(Center, Radius);
    }
}