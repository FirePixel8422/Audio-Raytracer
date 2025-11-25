using UnityEngine;
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
    [NoAlias] public NativeArray<AudioTargetRTData> AudioTargetRTData;

    [ReadOnly][NoAlias] public NativeArray<ushort> MuffleRayHits;

    [ReadOnly][NoAlias] public NativeArray<half3> EchoRayDirections;

    [ReadOnly][NoAlias] public int MaxRayHits;
    [ReadOnly][NoAlias] public int RayCount;
    [ReadOnly][NoAlias] public float3 RayOriginWorld;

    [ReadOnly][NoAlias] public float3 ListenerRightDir;



    [BurstCompile]
    public void Execute()
    {
        int totalRayResults = 0;

        for (int i = 0; i < TotalAudioTargets; i++)
        {
            AudioTargetRTData data = AudioTargetRTData[i];
            data.TargetHitCounts = 0;
            data.TargetReturnCounts = 0;
            data.TargetReturnPositionsTotal = float3.zero;
            data.TempTargetReturnPositions = float3.zero;
            AudioTargetRTData[i] = data;
        }

        int resultSetSize;
        AudioRayResult result;
        int lastRayAudioTargetId;

        //collect hit counts, direction sums, and return positions
        for (int i = 0; i < RayCount; i++)
        {
            resultSetSize = RayResultCounts[i];

            //add result count of this raySet to totalRayResults
            totalRayResults += resultSetSize;

            for (int bounceIndex = 0; bounceIndex < resultSetSize; bounceIndex++)
            {
                result = RayResults[i * MaxRayHits + bounceIndex];

                //if hitting any target increase hit count for that target id by 1
                if (result.AudioTargetId != -1)
                {
                    // >>> CHANGED: write into struct
                    var d = AudioTargetRTData[result.AudioTargetId];
                    d.TargetHitCounts += 1;
                    AudioTargetRTData[result.AudioTargetId] = d;
                }

                //final bounce of this ray their hit targetId (could be nothing aka -1)
                lastRayAudioTargetId = RayResults[i * MaxRayHits + resultSetSize - 1].AudioTargetId;

                // Check if this ray got to a audiotarget and if this bounce returned to origin (non-zero return direction)
                if (lastRayAudioTargetId != -1 && math.distance(EchoRayDirections[i], float3.zero) != 0)
                {
                    Half3.Divide(result.DEBUG_HitPoint, result.FullRayDistance != 0 ? result.FullRayDistance : (half3)1, out half3 output);

                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    // Multiply by 125 / 2 (62.5)?
                    Half3.Multiply(output, (half3)62.5f, out output);

                    // >>> CHANGED: write temp position + count into struct
                    AudioTargetRTData data = AudioTargetRTData[lastRayAudioTargetId];
                    data.TempTargetReturnPositions = (float3)output;
                    data.TargetReturnCounts += 1;
                    AudioTargetRTData[lastRayAudioTargetId] = data;

                    break;
                }
            }

            // Add last ray of every rayset that could retrace to origin
            for (int audioTargetId = 0; audioTargetId < TotalAudioTargets; audioTargetId++)
            {
                // >>> CHANGED: do accumulation in struct
                AudioTargetRTData data = AudioTargetRTData[audioTargetId];
                data.TargetReturnPositionsTotal += data.TempTargetReturnPositions;
                data.TempTargetReturnPositions = float3.zero;
                AudioTargetRTData[audioTargetId] = data;
            }
        }

        float strength;
        float pan;
        float muffle;

        float hitFraction;
        int maxBatchSize = MuffleRayHits.Length / TotalAudioTargets;

        //calculate audio strength and panstero based on newly calculated data
        for (int audioTargetId = 0; audioTargetId < TotalAudioTargets; audioTargetId++)
        {
            AudioTargetRTData data = AudioTargetRTData[audioTargetId];
            int totalMuffleRayhits = 0;

            //combine all spread muffleRayHitCount values for current audioTarget to 1 int (totalMuffleRayhits)
            for (int i = 0; i < maxBatchSize; i++)
            {
                totalMuffleRayhits += MuffleRayHits[TotalAudioTargets * i + audioTargetId];
            }
            //set muffleRayHits of current audiotargetId to the totalMuffleRayhits
            muffle = (float)totalMuffleRayhits / (RayCount * MaxRayHits);


            //if audiotarget was hit by at least 1 ray
            if (data.TargetHitCounts > 0)
            {
                hitFraction = (float)data.TargetHitCounts / RayCount;

                strength = math.saturate(hitFraction * 6); // If 16% of rays hit = full volume
            }
            //no rays hit audiotarget > 0 sound
            else
            {
                strength = 0;
            }

            // If we have return positions, use those to compute average direction
            if (data.TargetReturnCounts > 0)
            {
                float3 avgPos = (float3)data.TargetReturnPositionsTotal / data.TargetReturnCounts;

                // Calculate direction from listener to sound source (target direction)
                float3 targetDir = math.normalize(RayOriginWorld - avgPos); // Direction from listener to sound source

                // Project the target direction onto the horizontal plane (ignore y-axis)
                targetDir.y = 0f;

                // Calculate pan as a value between -1 (left) and 1 (right)
                pan = math.clamp(math.dot(targetDir, ListenerRightDir), -1, 1);
            }
            else
            {
                // Set value to -2 >> Null
                pan = -2;
            }

            // >>> CHANGED: write settings inside struct
            data.AudioTargetSettings = new AudioTargetSettings(
                strength,
                1 - muffle,
                0,
                0,
                pan,
                AudioTargetPositions[audioTargetId] - RayOriginWorld
            );

            AudioTargetRTData[audioTargetId] = data;
        }
    }
}