using System.Runtime.CompilerServices;
using Unity.Mathematics;


[System.Serializable]
public struct ReverbDSP
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Process(float[] data, MinMaxFloat reverbDryboost, float reverbBlend)
    {
        float dryBoost = math.lerp(reverbDryboost.min, reverbDryboost.max, reverbBlend);

        // Apply modifications to every sample of current audiosample buffer
        for (int i = 0; i < data.Length; i += 2)
        {
            float leftSample = data[i];
            float rightSample = data[i + 1];

            data[i] = leftSample * dryBoost;
            data[i + 1] = rightSample * dryBoost;
        }
    }
}
