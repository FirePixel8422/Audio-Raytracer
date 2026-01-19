using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[System.Serializable]
public struct AudioTargetRTSettings
{
    [Range(0, 1)]
    public float MuffleStrength;
    [Range(0, 1)]
    public float ReverbStrength;
    [Range(0, 1)]
    public float ReverbVolume;
    public float3 PercievedAudioPosition;

    public AudioTargetRTSettings(float muffleStrength, float reverbStrength, float reverbVolume, float3 percievedAudioPosition)
    {
        MuffleStrength = math.saturate(muffleStrength);
        ReverbStrength = math.saturate(reverbStrength);
        ReverbVolume = math.saturate(reverbVolume);
        PercievedAudioPosition = percievedAudioPosition;
    }
}