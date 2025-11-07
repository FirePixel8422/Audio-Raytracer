using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
public static class HalfDataTypesUtility
{
    public static half3 ToHalf3(this Vector3 value)
    {
        return new half3((half)value.x, (half)value.y, (half)value.z);
    }
    public static half3 ToHalf3(this float3 value)
    {
        return new half3((half)value.x, (half)value.y, (half)value.z);
    }
    public static float3 ToFloat3(this half3 value)
    {
        return new float3((float)value.x, (float)value.y, (float)value.z);
    }

    [BurstCompile]
    public static void ConvertToHalf3(in this Vector3 value, out half3 output)
    {
        output = new half3((half)value.x, (half)value.y, (half)value.z);
    }
    [BurstCompile]
    public static void ConvertToHalf3(in this float3 value, out half3 output)
    {
        output = new half3((half)value.x, (half)value.y, (half)value.z);
    }
    [BurstCompile]
    public static void ConvertToFloat3(in this half3 value, out float3 output)
    {
        output = new float3((float)value.x, (float)value.y, (float)value.z);
    }
}


[BurstCompile]
public struct Half3
{
    [BurstCompile]
    public static void Add(in half3 a, in half3 b, out half3 output)
    {
        output = new half3((half)(a.x + b.x), (half)(a.y + b.y), (half)(a.z + b.z));
    }
    [BurstCompile]
    public static void Add(in float3 a, in float3 b, out half3 output)
    {
        output = new half3((half)(a.x + b.x), (half)(a.y + b.y), (half)(a.z + b.z));
    }

    [BurstCompile]
    public static void Multiply(in half3 a, in half3 b, out half3 output)
    {
        output = new half3((half)(a.x * b.x), (half)(a.y * b.y), (half)(a.z * b.z));
    }
    [BurstCompile]
    public static void Multiply(in float3 a, in float3 b, out half3 output)
    {
        output = new half3((half)(a.x * b.x), (half)(a.y * b.y), (half)(a.z * b.z));
    }
}
[BurstCompile]
public struct Half
{
    [BurstCompile]
    public static void Add(in half a, in half b, out half output)
    {
        output = new half((half)(a + b));
    }
    [BurstCompile]
    public static void Add(in float a, in float b, out half output)
    {
        output = new half((half)a + b);
    }

    [BurstCompile]
    public static void Multiply(in half a, in half b, out half output)
    {
        output = new half((half)(a * b));
    }
    [BurstCompile]
    public static void Multiply(in float a, in float b, out half output)
    {
        output = new half((half)(a * b));
    }
}