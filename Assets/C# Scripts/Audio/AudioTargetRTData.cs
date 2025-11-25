using Unity.Burst;
using Unity.Mathematics;



[System.Serializable]
[BurstCompile]
public struct AudioTargetRTData
{
    public int TargetHitCounts;
    public float3 TargetReturnPositionsTotal;
    public float3 TempTargetReturnPositions;
    public int TargetReturnCounts;

    public AudioTargetSettings AudioTargetSettings;
}