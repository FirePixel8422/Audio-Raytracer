using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile, Serializable]
public struct ColliderSphereStruct
{
    public half3 Center;
    public half Radius;

    [HideInInspector] public AudioMaterialProperties MaterialProperties;
    [HideInInspector] public short AudioTargetId;


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
               a.MaterialProperties == b.MaterialProperties &&
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