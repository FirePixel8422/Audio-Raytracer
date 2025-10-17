using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile(DisableSafetyChecks = true)]
public static class MathLogic
{
    public static float DistanceFrom(this float3 value, float3 toSubtract)
    {
        float3 difference = value - toSubtract;
        return math.abs(difference.x) + math.abs(difference.y) + math.abs(difference.z);
    }
    public static int DistanceFrom(this int3 value, int3 toSubtract)
    {
        int3 difference = value - toSubtract;
        return math.abs(difference.x) + math.abs(difference.y) + math.abs(difference.z);
    }
    /// <returns>Absolute of: X + Y + Z</returns>
    public static float AbsoluteSum(this float3 value)
    {
        return math.abs(value.x) + math.abs(value.y) + math.abs(value.z);
    }


    public static bool IsZero(this Vector3 value)
    {
        return value.x == 0f && value.y == 0f && value.z == 0f;
    }
    public static bool IsZero(this Vector2 value)
    {
        return value.x == 0f && value.y == 0;
    }
    public static bool IsZero(this float3 value)
    {
        return value.x == 0f && value.y == 0f && value.z == 0f;
    }
    public static bool IsZero(this float2 value)
    {
        return value.x == 0f && value.y == 0f;
    }
    public static bool IsZero(this int3 value)
    {
        return value.x == 0f && value.y == 0f && value.z == 0f;
    }
    public static bool IsZero(this int2 value)
    {
        return value.x == 0f && value.y == 0f;
    }

    /// <summary>
    /// Clamps float between 0 and 1.
    /// </summary>
    public static void Saturated(this ref float value)
    {
        value = math.saturate(value);
    }

    /// <summary>
    /// Clamp float to 0 or more
    /// </summary>
    [BurstCompile(DisableSafetyChecks = true)]
    public static float ClampMin0(float value)
    {
        return 0 > value ? 0 : value;
    }


    /// <summary>
    /// Move float <paramref name="current"/> to <paramref name="target"/> with max step of <paramref name="maxDelta"/>
    /// </summary>
    [BurstCompile(DisableSafetyChecks = true)]
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        float delta = target - current;
        if (math.abs(delta) <= maxDelta) return target;
        return current + math.sign(delta) * maxDelta;
    }

    [BurstCompile(DisableSafetyChecks = true)]
    public static int ConvertToPowerOf2(int input)
    {
        if (input == 0) return 0;
        if (input == 1) return 1;
        if (input == 2) return 2;
        return 1 << (input - 1); // 2^(input-1)
    }

    public static MinMaxFloat Lerp(MinMaxFloat a, MinMaxFloat b, float t)
    {
        return new MinMaxFloat(math.lerp(a.min, b.max, t), math.lerp(a.max, b.max, t));
    }
}
public struct Float2
{
    public static float2 Zero => new float2(0f, 0f);
    public static float2 One => new float2(1f, 1f);
    public static float2 Left => new float2(-1f, 0f);
    public static float2 Right => new float2(1f, 0f);
    public static float2 Up => new float2(0f, 1f);
    public static float2 Down => new float2(0f, -1f);
}
public struct Int3
{
    public static int3 Zero => new int3(0, 0, 0);
    public static int3 One => new int3(1, 1, 1);
    public static int3 Up => new int3(0, 1, 0);
    public static int3 Down => new int3(0, -1, 0);
    public static int3 Left => new int3(-1, 0, 0);
    public static int3 Right => new int3(1, 0, 0);
    public static int3 Forward => new int3(0, 0, 1);
    public static int3 Backward => new int3(0, 0, -1);
}
public struct Int2
{
    public static int2 Zero => new int2(0, 0);
    public static int2 One => new int2(1, 1);
    public static int2 Left => new int2(-1, 0);
    public static int2 Right => new int2(1, 0);
    public static int2 Up => new int2(0, 1);
    public static int2 Down => new int2(0, -1);
}