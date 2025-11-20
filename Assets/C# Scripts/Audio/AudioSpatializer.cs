using UnityEngine;
using Unity.Mathematics;
using System;


[RequireComponent(typeof(AudioSource))]
public class AudioSpatializer : MonoBehaviour
{
    [SerializeField] private AudioSpatializerSettingsSO settingsSO;
    [SerializeField] private AudioSpatializerSettings settings;

    [Header("References")]
    [SerializeField] private Transform listenerTransform, soundPosTransform;

    [Range(0.25f, 10f)]
    public float volumeMultiplier = 1;
    [Range(0f, 1f)]
    public float muffleStrength;

    [SerializeField] private float3 cachedLocalDir;
    [SerializeField] private float cachedListenerDistance;

    private int sampleRate;

#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private int totalAudioFrames;
    [SerializeField] private int audioFPS;
    [SerializeField] private int audioFrameTime;
#endif


    private void OnEnable() => UpdateScheduler.RegisterLateUpdate(OnLateUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterLateUpdate(OnLateUpdate);


    private void Awake()
    {
        settings = settingsSO == null ? AudioSpatializerSettings.Default : settingsSO.settings;
        sampleRate = AudioSettings.outputSampleRate;
    }


    private void OnLateUpdate()
    {
        float3 worldDir = soundPosTransform.position - listenerTransform.position;
        cachedLocalDir = math.normalize(listenerTransform.InverseTransformDirection(worldDir));

        cachedListenerDistance = math.length(soundPosTransform.position - listenerTransform.position);

#if UNITY_EDITOR
        audioFPS = (int)math.floor(totalAudioFrames / Time.time);
        audioFrameTime = (int)math.floor(1f / audioFPS * 1000);
#endif
    }


    #region Audio Processing (On Audio Thread)

    // Filter state
    private float previousLeftLP;
    private float previousRightLP;
    private float previousLeftHP;
    private float previousRightHP;
    private float previousLeftInput;
    private float previousRightInput;

    // New muffle pass filter state
    private float previousLeftMuffle;
    private float previousRightMuffle;

    private const float DoublePI = 2f * math.PI;


    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2)
            return;

#if UNITY_EDITOR
        totalAudioFrames += 1;
#endif

        float3 localDir = cachedLocalDir;
        float distanceToListener = cachedListenerDistance;
        float azimuth = math.degrees(math.atan2(localDir.x, localDir.z));

        float effectivePanStrength = settings.panStrength;
        if (settings.distanceBasedPanning)
        {
            float distanceFactor = math.saturate(distanceToListener / settings.maxPanDistance);
            effectivePanStrength *= distanceFactor;
        }

        float pan = math.sin(math.radians(azimuth)) * effectivePanStrength;
        float leftGain = math.sqrt(0.5f * (1f - pan));
        float rightGain = math.sqrt(0.5f * (1f + pan));

        float frontFactor = math.max(0f, math.cos(math.radians(azimuth)));
        float rearAtten = math.lerp(1f - settings.rearAttenuationStrength, 1f, frontFactor);

        if (settings.distanceBasedRearAttenuation)
        {
            rearAtten = math.clamp(rearAtten * math.saturate(1f - (distanceToListener / settings.maxRearAttenuationDistance)), 1f - settings.rearAttenuationStrength, 1f);
        }

        // Create a falloff factor based on elevation (localDir.y)
        float volumeFalloff = 1f;
        if (localDir.y <= 0f)
        {
            // Lowpass: as the sound goes below the horizon, reduce volume more
            volumeFalloff = math.lerp(1f, settings.lowPassVolume, math.saturate(-localDir.y)); // More lowpass = less volume
        }
        else
        {
            // Highpass: as the sound goes above the horizon, reduce volume more
            volumeFalloff = math.lerp(1f, settings.highPassVolume, math.saturate(localDir.y)); // More highpass = less volume
        }

        for (int i = 0; i < data.Length; i += 2)
        {
            float leftSample = data[i];
            float rightSample = data[i + 1];

            // Apply the volume falloff based on elevation
            float processedLeft = leftSample * leftGain * rearAtten * volumeMultiplier * volumeFalloff;
            float processedRight = rightSample * rightGain * rearAtten * volumeMultiplier * volumeFalloff;

            // Apply Lowpass if elevation is below horizon
            if (localDir.y <= 0f)
            {
                float lowPassCutoff = math.lerp(settings.lowPassCutoff.max, settings.lowPassCutoff.min, math.saturate(-localDir.y)) * (1f - 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

                processedLeft = LowPass(processedLeft, ref previousLeftLP, lowPassCutoff, sampleRate);
                processedRight = LowPass(processedRight, ref previousRightLP, lowPassCutoff, sampleRate);
            }
            // Apply Highpass if elevation is above horizon
            else
            {
                float highPassCutoff = math.lerp(settings.highPassCutoff.min, settings.highPassCutoff.max, math.saturate(localDir.y)) * (1f + 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

                processedLeft = HighPass(processedLeft, ref previousLeftInput, ref previousLeftHP, highPassCutoff, sampleRate);
                processedRight = HighPass(processedRight, ref previousRightInput, ref previousRightHP, highPassCutoff, sampleRate);
            }

            // Apply additional muffle lowpass based on muffleStrength
            if (muffleStrength > 0f)
            {
                float muffleCutoff = math.lerp(settings.muffleCutoff.max, settings.muffleCutoff.min, muffleStrength);
                processedLeft = LowPass(processedLeft, ref previousLeftMuffle, muffleCutoff, sampleRate);
                processedRight = LowPass(processedRight, ref previousRightMuffle, muffleCutoff, sampleRate);
            }

            data[i] = processedLeft;
            data[i + 1] = processedRight;
        }
    }

    private float LowPass(float input, ref float previousOutput, float cutoff, float sampleRate)
    {
        float RC = 1.0f / (cutoff * DoublePI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (RC + dt);
        previousOutput += alpha * (input - previousOutput);
        return previousOutput;
    }

    private float HighPass(float input, ref float previousInput, ref float previousOutput, float cutoff, float sampleRate)
    {
        float RC = 1.0f / (cutoff * DoublePI);
        float dt = 1.0f / sampleRate;
        float alpha = RC / (RC + dt);
        float output = alpha * (previousOutput + input - previousInput);
        previousInput = input;
        previousOutput = output;
        return output;
    }

    #endregion
}
