using System.Runtime.CompilerServices;
using Unity.Mathematics;


[System.Serializable]
public struct BinauralDSP
{
    // Filter states
    private StereoFloat previousLP;
    private StereoFloat previousHP;
    private StereoFloat previousInput;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Process(float[] data, AudioSpatializerSettings settings, int sampleRate, float3 localDir, float distanceToListener)
    {
        float azimuth = math.degrees(math.atan2(localDir.x, localDir.z));

        float effectivePanStrength = settings.PanStrength;
        if (settings.DistanceBasedPanning)
        {
            float distanceFactor = math.saturate(distanceToListener / settings.MaxPanDistance);
            effectivePanStrength *= distanceFactor;
        }

        // LR panning based on azimuth
        float pan = math.sin(math.radians(azimuth)) * effectivePanStrength;
        float leftGain = math.sqrt(0.5f * (1f - pan));
        float rightGain = math.sqrt(0.5f * (1f + pan));

        // Rear attenuation based on azimuth and distance
        float frontFactor = math.max(0f, math.cos(math.radians(azimuth)));
        float rearAtten = math.lerp(1f - settings.RearAttenuationStrength, 1f, frontFactor);

        if (settings.DistanceBasedRearAttenuation)
        {
            float distanceFactor = math.saturate(1f - (distanceToListener / settings.MaxRearAttenuationDistance));

            rearAtten = math.clamp(rearAtten * distanceFactor, 1f - settings.RearAttenuationStrength, 1f);
        }

        // Volume multiplier based on if target is above or below the listener
        float elevationVolumeMultiplier = localDir.y <= 0f ?
             math.lerp(1f, settings.LowPassVolume, math.saturate(-localDir.y)) :
             math.lerp(1f, settings.HighPassVolume, math.saturate(localDir.y));

        // Create a final sample modification multiplier float for left and right sample
        StereoFloat sampleModification = new StereoFloat(
            leftGain * rearAtten * elevationVolumeMultiplier,
            rightGain * rearAtten * elevationVolumeMultiplier);


        // Apply modifications to every sample of current audiosample buffer
        for (int i = 0; i < data.Length; i += 2)
        {
            float leftSample = data[i];
            float rightSample = data[i + 1];

            float processedLeft = leftSample * sampleModification.Left;
            float processedRight = rightSample * sampleModification.Right;

            // Apply Lowpass if elevation is below horizon
            if (localDir.y <= 0f)
            {
                float lowPassCutoff = math.lerp(settings.LowPassCutoff.min, settings.LowPassCutoff.max, math.saturate(-localDir.y)) * (1f - 0.5f * math.saturate(distanceToListener / settings.MaxElevationEffectDistance));
                
                processedLeft = LowPass(processedLeft, ref previousLP.Left, lowPassCutoff, sampleRate);
                processedRight = LowPass(processedRight, ref previousLP.Right, lowPassCutoff, sampleRate);
            }
            // Apply Highpass if elevation is above horizon
            else
            {
                float highPassCutoff = math.lerp(settings.HighPassCutoff.min, settings.HighPassCutoff.max, math.saturate(localDir.y)) * (1f + 0.5f * math.saturate(distanceToListener / settings.MaxElevationEffectDistance));

                processedLeft = HighPass(processedLeft, ref previousInput.Left, ref previousHP.Left, highPassCutoff, sampleRate);
                processedRight = HighPass(processedRight, ref previousInput.Right, ref previousHP.Right, highPassCutoff, sampleRate);
            }

            data[i] = processedLeft;
            data[i + 1] = processedRight;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float HighPass(float input, ref float previousInput, ref float previousOutput, float cutoff, float sampleRate)
    {
        float rc = 1.0f / (cutoff * DOUBLE_PI);
        float dt = 1.0f / sampleRate;
        float alpha = rc / (rc + dt);
        float output = alpha * (previousOutput + input - previousInput);
        previousInput = input;
        previousOutput = output;
        return output;
    }
}
