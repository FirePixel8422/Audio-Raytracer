using Unity.Burst;
using Unity.Mathematics;


[System.Serializable]
[BurstCompile]
public struct AudioRayResult
{
    public half Distance;
    public half FullRayDistance;


#if UNITY_EDITOR
    public half3 DEBUG_HitPoint;
#endif
}