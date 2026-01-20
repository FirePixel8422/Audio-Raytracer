using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile, Serializable]
public struct ColliderOBBStruct
{
    public half3 Center;
    public half3 Size;

    private halfQuaternion rotation;
    public quaternion Rotation
    {
        get => rotation.QuaternionValue;
        set
        {
            rotation.QuaternionValue = value;
        }
    }

    [HideInInspector] public AudioMaterialProperties MaterialProperties;
    [HideInInspector] public short AudioTargetId;


    public static ColliderOBBStruct Default => new ColliderOBBStruct()
    {
        Size = new half3(0.5f),
        rotation = new halfQuaternion(quaternion.identity),
        AudioTargetId = -1,
    };

    public static bool operator ==(ColliderOBBStruct a, ColliderOBBStruct b)
    {
        return a.Center.x.value == b.Center.x.value &&
               a.Center.y.value == b.Center.y.value &&
               a.Center.z.value == b.Center.z.value &&
               a.Size.x.value == b.Size.x.value &&
               a.Size.y.value == b.Size.y.value &&
               a.Size.z.value == b.Size.z.value &&
               a.rotation == b.rotation &&
               a.MaterialProperties == b.MaterialProperties &&
               a.AudioTargetId == b.AudioTargetId;
    }
    public static bool operator !=(ColliderOBBStruct a, ColliderOBBStruct b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        return obj is ColliderOBBStruct other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Center, Size, rotation);
    }
}