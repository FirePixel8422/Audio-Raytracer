using Unity.Burst;
using Unity.Mathematics;


[System.Serializable]
[BurstCompile]
public struct AudioRayResult
{
    public half Distance;
    public half FullRayDistance;
    public short AudioTargetId;

    /// <summary>
    /// If distance == -1, return true (is null)
    /// </summary>
    public bool IsNull => Distance == -1;

    public static AudioRayResult Null => new AudioRayResult
    {
        Distance = (half)(-1),
        AudioTargetId = -1,
    };


#if UNITY_EDITOR
    public half3 DEBUG_HitPoint;
#endif
}