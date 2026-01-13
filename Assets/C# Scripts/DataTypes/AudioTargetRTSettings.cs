using Unity.Burst;
using Unity.Mathematics;


[System.Serializable]
[BurstCompile]
public struct AudioTargetRTSettings
{
    public float Muffle;

    public float EchoStrength;
    public float EchoTime;

    public float3 Position;

    public AudioTargetRTSettings(float muffle, float echoStrength, float echoTime, float3 position)
    {
        Muffle = muffle;

        EchoStrength = echoStrength;
        EchoTime = echoTime;

        Position = position;
    }
}