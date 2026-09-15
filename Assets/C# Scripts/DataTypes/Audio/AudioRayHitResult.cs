#if UNITY_EDITOR
using Unity.Burst;
using Unity.Mathematics;


[System.Serializable]
[BurstCompile]
public struct AudioRayHitResult
{
    public half3 HitPoint;
}
#endif