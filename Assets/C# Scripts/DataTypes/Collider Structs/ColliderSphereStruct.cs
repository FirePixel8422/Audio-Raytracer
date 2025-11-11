using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 4)]
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

    public static bool operator ==(ColliderSphereStruct a, ColliderSphereStruct b)
    {
        return math.all(a.center == b.center) &&
               a.radius == b.radius &&
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