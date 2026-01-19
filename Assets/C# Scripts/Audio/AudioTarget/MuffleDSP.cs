using System.Runtime.CompilerServices;
using Unity.Mathematics;


[System.Serializable]
public struct MuffleDSP
{
    // New muffle pass filter state
    private StereoFloat previousMuffle;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Process(float[] data, NativeSampledAnimationCurve muffleCurve, MinMaxFloat muffleCutoffMinMax, int sampleRate, float muffleStrength)
    {
        // Apply modifications to every sample of current audiosample buffer
        for (int i = 0; i < data.Length; i += 2)
        {
            float leftSample = data[i];
            float rightSample = data[i + 1];

            // Apply additional muffle lowpass based on MuffleStrength
            if (muffleStrength > 0f)
            {
                float muffle = muffleCurve.Evaluate(muffleStrength);

                float muffleCutoff = math.lerp(muffleCutoffMinMax.max, muffleCutoffMinMax.min, muffle);

                data[i] = LowPass(leftSample, ref previousMuffle.Left, muffleCutoff, sampleRate);
                data[i + 1] = LowPass(rightSample, ref previousMuffle.Right, muffleCutoff, sampleRate);
            }
        }
    }


    private const float DOUBLE_PI = 2f * math.PI;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LowPass(float input, ref float previousOutput, float cutoff, float sampleRate)
    {
        float rc = 1.0f / (cutoff * DOUBLE_PI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (rc + dt);
        previousOutput += alpha * (input - previousOutput);
        return previousOutput;
    }
}