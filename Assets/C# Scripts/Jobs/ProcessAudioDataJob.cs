using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct ProcessAudioDataJob : IJob
{
    [ReadOnly][NoAlias] public NativeArray<AudioRayResult> RayResults;
    [ReadOnly][NoAlias] public NativeArray<byte> RayResultCounts;

    [ReadOnly][NoAlias] public int TotalAudioTargets;
    [ReadOnly][NoAlias] public NativeArray<float3> AudioTargetPositions;

    [ReadOnly][NoAlias] public NativeArray<ushort> MuffleRayHits;
    [ReadOnly][NoAlias] public NativeArray<half> EchoRayDistances;

    [ReadOnly][NoAlias] public int MaxHitsPerRay;
    [ReadOnly][NoAlias] public int RayCount;
    [ReadOnly][NoAlias] public float3 RayOriginWorld;

    [WriteOnly][NoAlias] public NativeArray<AudioTargetRTSettings> AudioTargetSettings;


    [BurstCompile]
    public void Execute()
    {
        float muffle;
        int maxBatchSize = MuffleRayHits.Length / TotalAudioTargets;

        //calculate audio strength and panstero based on newly calculated data
        for (int audioTargetId = 0; audioTargetId < TotalAudioTargets; audioTargetId++)
        {
            int totalMuffleRayhits = 0;

            // Combine all spread muffleRayHitCount values for current audioTarget to 1 int (totalMuffleRayhits)
            for (int i = 0; i < maxBatchSize; i++)
            {
                totalMuffleRayhits += MuffleRayHits[TotalAudioTargets * i + audioTargetId];
            }

            // Set muffleRayHits of current audiotargetId to the totalMuffleRayhits
            muffle = (float)totalMuffleRayhits / (RayCount * MaxHitsPerRay);

            // Write new settings back to array
            AudioTargetSettings[audioTargetId] = new AudioTargetRTSettings(1 - muffle, 0, 0, AudioTargetPositions[audioTargetId] - RayOriginWorld);
        }
    }
}