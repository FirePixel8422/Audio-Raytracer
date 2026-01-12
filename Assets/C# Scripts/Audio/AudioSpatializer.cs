using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class AudioSpatializer : MonoBehaviour
{
    [SerializeField] private AudioSpatializerSettingsSO settingsSO;
    [SerializeField] private AudioSpatializerSettings settings;

    [Header("References")]
    [SerializeField] private Transform listenerTransform, soundPosTransform;

    [Range(0, 5)]
    [SerializeField] private float volumeMultiplier = 1;

    [Range(0, 1)]
    public float MuffleStrength;

    [Range(0, 1)]
    public float ReverbBlend;
    private SimpleReverb reverb;

    private float3 cachedLocalDir;
    private float cachedListenerDistance;
    
    private int sampleRate;
    
    private const float DoublePI = 2f * math.PI;



    private void OnEnable() => UpdateScheduler.RegisterLateUpdate(OnLateUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterLateUpdate(OnLateUpdate);

    private void Awake()
    {
        settings = settingsSO == null ? AudioSpatializerSettings.Default : settingsSO.settings;

        sampleRate = AudioSettings.outputSampleRate;
        reverb = new SimpleReverb(sampleRate, settings.reverbDecayFactor, settings.reverbAllpassGain);
    }


    private void OnLateUpdate()
    {
        float3 worldDir = soundPosTransform.position - listenerTransform.position;
        cachedLocalDir = math.normalize(listenerTransform.InverseTransformDirection(worldDir));

        cachedListenerDistance = math.length(soundPosTransform.position - listenerTransform.position);
    }


    #region Audio Processing (On Audio Thread)

    // Filter state
    private StereoFloat previousLP;
    private StereoFloat previousHP;
    private StereoFloat previousInput;

    // New muffle pass filter state
    private StereoFloat previousMuffle;


    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2) return;

        float3 localDir = cachedLocalDir;
        float distanceToListener = cachedListenerDistance;
        float azimuth = math.degrees(math.atan2(localDir.x, localDir.z));


        #region Panning Calculation

        float effectivePanStrength = settings.panStrength;
        if (settings.distanceBasedPanning)
        {
            float distanceFactor = math.saturate(distanceToListener / settings.maxPanDistance);
            effectivePanStrength *= distanceFactor;
        }

        float pan = math.sin(math.radians(azimuth)) * effectivePanStrength;
        float leftGain = math.sqrt(0.5f * (1f - pan));
        float rightGain = math.sqrt(0.5f * (1f + pan));

        #endregion


        #region Rear Attenuation (Based on Azimuth and Distance)

        float frontFactor = math.max(0f, math.cos(math.radians(azimuth)));
        float rearAtten = math.lerp(1f - settings.rearAttenuationStrength, 1f, frontFactor);

        if (settings.distanceBasedRearAttenuation)
        {
            rearAtten = math.clamp(rearAtten * math.saturate(1f - (distanceToListener / settings.maxRearAttenuationDistance)), 1f - settings.rearAttenuationStrength, 1f);
        }

        #endregion


        #region Sound Multiplier based on Elevation

        // Modify sound volume based on if targetr is above or below the listener
        float volumeFalloff = 1f;
        if (localDir.y <= 0f)
        {
            // Volume modifier for Lowpass
            volumeFalloff = math.lerp(1f, settings.lowPassVolume, math.saturate(-localDir.y));
        }
        else
        {
            // Volume modifier for Highpass
            volumeFalloff = math.lerp(1f, settings.highPassVolume, math.saturate(localDir.y));
        }

        #endregion


        // Apply modifications to every sample of current audiosample buffer
        for (int i = 0; i < data.Length; i += 2)
        {
            float leftSample = data[i];
            float rightSample = data[i + 1];

            // Apply the volume falloff based on elevation
            float processedLeft = leftSample * leftGain * rearAtten * volumeMultiplier * volumeFalloff;
            float processedRight = rightSample * rightGain * rearAtten * volumeMultiplier * volumeFalloff;


            #region LowPass/HighPass based on elevation

            // Apply Lowpass if elevation is below horizon
            if (localDir.y <= 0f)
            {
                float lowPassCutoff = math.lerp(settings.lowPassCutoff.max, settings.lowPassCutoff.min, math.saturate(-localDir.y)) * (1f - 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

                processedLeft = LowPass(processedLeft, ref previousLP.Left, lowPassCutoff, sampleRate);
                processedRight = LowPass(processedRight, ref previousLP.Right, lowPassCutoff, sampleRate);
            }
            // Apply Highpass if elevation is above horizon
            else
            {
                float highPassCutoff = math.lerp(settings.highPassCutoff.min, settings.highPassCutoff.max, math.saturate(localDir.y)) * (1f + 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

                processedLeft = HighPass(processedLeft, ref previousInput.Left, ref previousHP.Left, highPassCutoff, sampleRate);
                processedRight = HighPass(processedRight, ref previousInput.Right, ref previousHP.Right, highPassCutoff, sampleRate);
            }

            #endregion


            #region Muffle (Lowpass based on MuffleStrength)

            // Apply additional muffle lowpass based on MuffleStrength
            if (MuffleStrength > 0f)
            {
                float muffleCutoff = math.lerp(settings.muffleCutoff.max, settings.muffleCutoff.min, MuffleStrength);
                processedLeft = LowPass(processedLeft, ref previousMuffle.Left, muffleCutoff, sampleRate);
                processedRight = LowPass(processedRight, ref previousMuffle.Right, muffleCutoff, sampleRate);
            }

            #endregion


            #region Reverb
            
            if (ReverbBlend > 0)
            {
                float dryL = processedLeft;
                float dryR = processedRight;

                float wetL = reverb.Process(dryL) * settings.wetBoostMultiplier;
                float wetR = reverb.Process(dryR) * settings.wetBoostMultiplier;

                processedLeft = math.lerp(dryL, wetL, ReverbBlend);
                processedRight = math.lerp(dryR, wetR, ReverbBlend);
            }
            
            #endregion


            data[i] = processedLeft;
            data[i + 1] = processedRight;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LowPass(float input, ref float previousOutput, float cutoff, float sampleRate)
    {
        float RC = 1.0f / (cutoff * DoublePI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (RC + dt);
        previousOutput += alpha * (input - previousOutput);
        return previousOutput;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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



#if UNITY_EDITOR
    [SerializeField] private bool DEBUG_SyncSettingsToSO;

    private void OnValidate()
    {
        if (settings.wetBoostMultiplier != 0 && Application.isPlaying && DEBUG_SyncSettingsToSO && settingsSO != null)
        {
            settingsSO.settings = settings;
        }
    }
#endif
}
