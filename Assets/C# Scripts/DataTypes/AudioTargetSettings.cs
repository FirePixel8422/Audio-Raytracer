using Unity.Burst;
using Unity.Mathematics;



[System.Serializable]
[BurstCompile]
public struct AudioTargetSettings
{
    public float volume;
    public float muffle;

    public float echoStrength;
    public float echoTime;

    public float3 position;

    public AudioTargetSettings(float volume, float muffle, float echoStrength, float echoTime, float3 position)
    {
        this.volume = volume;
        this.muffle = muffle;

        this.echoStrength = echoStrength;
        this.echoTime = echoTime;

        this.position = position;
    }

    public AudioTargetSettings(AudioTargetSettings newSettings)
    {
        volume = newSettings.volume;
        muffle = newSettings.muffle;

        echoStrength = newSettings.echoStrength;
        echoTime = newSettings.echoTime;

        position = newSettings.position;
    }
}