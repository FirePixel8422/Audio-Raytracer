using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct AudioTargetRTSettings
{
    [Range(0, 1)]
    public float MuffleStrength;
    [Range(0, 1)]
    public float ReverbBlend;
    public float3 PercievedAudioPosition;

    public AudioTargetRTSettings(float muffleStrength, float reverbBlend, float3 percievedAudioPosition)
    {
        MuffleStrength = math.saturate(muffleStrength);
        ReverbBlend = math.saturate(reverbBlend);
        PercievedAudioPosition = percievedAudioPosition;
    }
}