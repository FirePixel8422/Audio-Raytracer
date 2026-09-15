using Fire_Pixel.Utility;
using System;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public struct ColliderAABBStruct
{
    public half3 Center;
    public half3 Size;

    //[EditorReadOnly] public AudioMaterialProperties MaterialProperties;
    [EditorReadOnly] public short MaterialId;
    [HideInInspector] public short AudioTargetId;


    public static ColliderAABBStruct Default => new ColliderAABBStruct()
    {
        Size = new half3(0.5f),
        AudioTargetId = -1,
    };

    public static bool operator ==(ColliderAABBStruct a, ColliderAABBStruct b)
    {
        return a.Center.x.value == b.Center.x.value &&
               a.Center.y.value == b.Center.y.value &&
               a.Center.z.value == b.Center.z.value &&
               a.Size.x.value == b.Size.x.value &&
               a.Size.y.value == b.Size.y.value &&
               a.Size.z.value == b.Size.z.value &&
               a.MaterialId == b.MaterialId &&
               a.AudioTargetId == b.AudioTargetId;
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
        return HashCode.Combine(Center, Size);
    }
}