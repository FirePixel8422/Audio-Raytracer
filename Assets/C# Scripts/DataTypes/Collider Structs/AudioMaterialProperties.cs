using System;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct AudioMaterialProperties
{
    [Tooltip("How much power of the mainRays hitting this surface gets consumed")]
    public half Absorption;

    [Tooltip("How much power gets consumed when permeation rays go through material")]
    public half Density;

    [Tooltip("How much echo power gets added to echo rays upon collision")]
    public half Echo;


    public static bool operator ==(AudioMaterialProperties a, AudioMaterialProperties b)
    {
        return a.Density.value == b.Density.value &&
               a.Absorption.value == b.Absorption.value &&
               a.Echo.value == b.Echo.value;
    }
    public static bool operator !=(AudioMaterialProperties a, AudioMaterialProperties b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        return obj is AudioMaterialProperties other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Absorption, Density, Echo);
    }
}