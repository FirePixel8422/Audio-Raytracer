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
    [ReadOnly][NoAlias] public float MaxReverbDistance;

    [ReadOnly][NoAlias] public int MaxHitsPerRay;
    [ReadOnly][NoAlias] public int RayCount;
    [ReadOnly][NoAlias] public float3 RayOriginWorld;

    [WriteOnly][NoAlias] public NativeArray<AudioTargetRTSettings> AudioTargetSettings;


    [BurstCompile]
    public void Execute()
    {
        int maxBatchSize = MuffleRayHits.Length / TotalAudioTargets;
        int maxRayHits = MaxHitsPerRay * RayCount;

        // Calculate avarage echo distance
        float reverbTotal = 0;
        for (int i = 0; i < maxRayHits; i++)
        {
            reverbTotal += EchoRayDistances[i];
        }
        float avgReverbDist = reverbTotal / maxRayHits;
        float reverbBlend = avgReverbDist / MaxReverbDistance;


        // Calculate audio strength and panstero based on newly calculated data
        for (int audioTargetId = 0; audioTargetId < TotalAudioTargets; audioTargetId++)
        {
            int totalMuffleRayhits = 0;

            // Combine all spread muffleRayHitCount values for current audioTarget to 1 int (totalMuffleRayhits)
            for (int i = 0; i < maxBatchSize; i++)
            {
                totalMuffleRayhits += MuffleRayHits[TotalAudioTargets * i + audioTargetId];
            }

            // Set muffleRayHits of current audiotargetId to the totalMuffleRayhits
            float muffle = (float)totalMuffleRayhits / (RayCount * MaxHitsPerRay);

            // Write new settings back to array
            AudioTargetSettings[audioTargetId] = new AudioTargetRTSettings(1 - muffle, reverbBlend, AudioTargetPositions[audioTargetId] - RayOriginWorld);
        }
    }
}