//using UnityEngine;
//using Unity.Mathematics;
//using System.Runtime.CompilerServices;
//using Unity.Collections;


//[RequireComponent(typeof(AudioSource))]
//public class AudioSpatializerNew : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform listenerTransform;

//    [Range(0.25f, 10f)]
//    public float overallGain;

//    [SerializeField] private AudioSpatializerSettingsSO audioSpatializerSettingsSO;
//    [SerializeField] private AudioSpatializerSettings settings;


//    private NativeArray<StereoFloat> reverbBuffer;
//    private int bufferSizePerTap;

//    private int[] reverbBufferOffsets; // start index per tap in reverbBufferLR
//    private int[] reverbBufferIndices;
//    private float[] tapDelayMultipliers;

//    private float[] previousReverbLP_Left = new float[ReverbTapCount];
//    private float[] previousReverbLP_Right = new float[ReverbTapCount];

//    [SerializeField] private float reverbTime;

//    private float3 cachedLocalDir;
//    private float3 listenerPosition;

//    [SerializeField] private float3 soundOffsetToPlayer;

//    // Filter state
//    private StereoFloat previousLP;
//    private StereoFloat previousHP;
//    private StereoFloat previousInput;

//    // New muffle pass filter state
//    private StereoFloat previousMuffle;

//    private int sampleRate;

//    private const int ReverbTapCount = 4;
//    private const float DoublePI = 2f * math.PI;



//    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
//    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);


//    private void Start()
//    {
//        sampleRate = AudioSettings.outputSampleRate;
//        settings = audioSpatializerSettingsSO != null ? audioSpatializerSettingsSO.settings : AudioSpatializerSettings.Default;

//        reverbBufferOffsets = new int[ReverbTapCount];
//        reverbBufferIndices = new int[ReverbTapCount];
//        tapDelayMultipliers = new float[ReverbTapCount];

//        bufferSizePerTap = Mathf.CeilToInt(sampleRate * settings.maxReverbTime);
//        int totalBufferSize = bufferSizePerTap * ReverbTapCount;

//        for (int i = 0; i < ReverbTapCount; i++)
//        {
//            reverbBufferOffsets[i] = i * bufferSizePerTap;
//            reverbBufferIndices[i] = 0;
//            tapDelayMultipliers[i] = EzRandom.Range(0.1f, 0.9f); // random per tap delay fraction
//        }

//        reverbBuffer = new NativeArray<StereoFloat>(totalBufferSize, Allocator.Persistent);

//#if UNITY_EDITOR
//        cachedMaxReverbTime = settings.maxReverbTime;
//#endif
//    }



//    private void OnUpdate()
//    {
//        if (listenerTransform != null)
//        {
//            listenerPosition = listenerTransform.position;

//            float3 worldDir = soundOffsetToPlayer + listenerPosition;
//            cachedLocalDir = math.normalize(listenerTransform.InverseTransformDirection(worldDir));
//        }
//    }

//    public void UpdateSettings(AudioTargetData uudioTargetData)
//    {
//        settings.muffleStrength = uudioTargetData.muffle;
//        soundOffsetToPlayer = uudioTargetData.position;
//    }


//    private void OnAudioFilterRead(float[] data, int channels)
//    {
//        if (channels != 2)
//            return;

//        float3 localDir = cachedLocalDir;
//        float distanceToListener = math.length(soundOffsetToPlayer);
//        float azimuth = math.atan2(localDir.x, localDir.z); // radians

//        float effectivePanStrength = settings.panStrength;
//        if (settings.distanceBasedPanning)
//        {
//            float distanceFactor = math.saturate(distanceToListener / settings.maxPanDistance);
//            effectivePanStrength *= distanceFactor;
//        }

//        float pan = math.sin(azimuth) * effectivePanStrength;
//        float leftGain = math.sqrt(0.5f * (1f - pan));
//        float rightGain = math.sqrt(0.5f * (1f + pan));

//        float frontFactor = math.max(0f, math.cos(azimuth));
//        float rearAtten = math.lerp(1f - settings.rearAttenuationStrength, 1f, frontFactor);

//        if (settings.distanceBasedRearAttenuation)
//        {
//            rearAtten = math.clamp(rearAtten * math.saturate(1f - (distanceToListener / settings.maxRearAttenuationDistance)), 1f - settings.rearAttenuationStrength, 1f);
//        }

//        float volumeFalloff;
//        if (localDir.y <= 0f)
//        {
//            volumeFalloff = math.lerp(1f, settings.lowPassVolume, math.saturate(-localDir.y));
//        }
//        else
//        {
//            volumeFalloff = math.lerp(1f, settings.highPassVolume, math.saturate(localDir.y));
//        }

//        for (int i = 0; i < data.Length; i += 2)
//        {
//            float leftSample = data[i];
//            float rightSample = data[i + 1];

//            float processedLeft = leftSample * leftGain * rearAtten * overallGain * volumeFalloff;
//            float processedRight = rightSample * rightGain * rearAtten * overallGain * volumeFalloff;

//            // Elevation effects
//            if (localDir.y <= 0f)
//            {
//                float lowPassCutoff = math.lerp(settings.lowPassCutoff.min, settings.lowPassCutoff.max, math.saturate(-localDir.y))
//                                    * (1f - 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

//                processedLeft = LowPass(processedLeft, ref previousLP.Left, lowPassCutoff, sampleRate);
//                processedRight = LowPass(processedRight, ref previousLP.Right, lowPassCutoff, sampleRate);
//            }
//            else
//            {
//                float highPassCutoff = math.lerp(settings.highPassCutoff.min, settings.highPassCutoff.max, math.saturate(localDir.y))
//                                     * (1f + 0.5f * math.saturate(distanceToListener / settings.maxElevationEffectDistance));

//                processedLeft = HighPass(processedLeft, ref previousInput.Left, ref previousHP.Left, highPassCutoff, sampleRate);
//                processedRight = HighPass(processedRight, ref previousInput.Right, ref previousHP.Right, highPassCutoff, sampleRate);
//            }

//            // Muffle effect
//            if (settings.muffleStrength > 0f)
//            {
//                float muffleCutoff = math.lerp(settings.muffleCutoff.min, settings.muffleCutoff.max, settings.muffleStrength);
//                processedLeft = LowPass(processedLeft, ref previousMuffle.Left, muffleCutoff, sampleRate);
//                processedRight = LowPass(processedRight, ref previousMuffle.Right, muffleCutoff, sampleRate);
//            }

//            // Reverb Effect
//            if (reverbTime > 0)
//            {
//                for (int tap = 0; tap < ReverbTapCount; tap++)
//                {
//                    int writeIndex = reverbBufferIndices[tap];

//                    // Calculate delay samples based on current reverbTime and per-tap multiplier
//                    float tapDelaySec = reverbTime * tapDelayMultipliers[tap];
//                    int delaySamples = Mathf.FloorToInt(sampleRate * tapDelaySec);

//                    // Calculate read index in circular buffer
//                    int readIndex = (writeIndex - delaySamples + bufferSizePerTap) % bufferSizePerTap;

//                    int bufferOffset = reverbBufferOffsets[tap];
//                    int readPos = bufferOffset + readIndex;
//                    int writePos = bufferOffset + writeIndex;

//                    // Get delayed sample from buffer
//                    StereoFloat delayedSample = reverbBuffer[readPos];

//                    // Apply a mild lowpass filter on delayed sample per channel
//                    float lowPassCutoff = 3000f; // tweak this freq to taste (Hz)
//                    delayedSample.Left = LowPass(delayedSample.Left, ref previousReverbLP_Left[tap], lowPassCutoff, sampleRate);
//                    delayedSample.Right = LowPass(delayedSample.Right, ref previousReverbLP_Right[tap], lowPassCutoff, sampleRate);

//                    // Mix filtered delayed sample scaled by decay
//                    processedLeft += delayedSample.Left * settings.reverbDecay;
//                    processedRight += delayedSample.Right * settings.reverbDecay;

//                    // Write the current processed sample to the buffer at writePos
//                    reverbBuffer[writePos] = new StereoFloat(processedLeft, processedRight);

//                    // Advance write index with wrap-around
//                    reverbBufferIndices[tap] = (writeIndex + 1) % bufferSizePerTap;
//                }
//            }

//            data[i] = processedLeft;
//            data[i + 1] = processedRight;
//        }
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private float LowPass(float input, ref float previousOutput, float cutoff, float sampleRate)
//    {
//        float RC = 1.0f / (cutoff * DoublePI);
//        float dt = 1.0f / sampleRate;
//        float alpha = dt / (RC + dt);
//        previousOutput += alpha * (input - previousOutput);
//        return previousOutput;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private float HighPass(float input, ref float previousInput, ref float previousOutput, float cutoff, float sampleRate)
//    {
//        float RC = 1.0f / (cutoff * DoublePI);
//        float dt = 1.0f / sampleRate;
//        float alpha = RC / (RC + dt);
//        float output = alpha * (previousOutput + input - previousInput);
//        previousInput = input;
//        previousOutput = output;
//        return output;
//    }

//    private void OnDestroy()
//    {
//        reverbBuffer.DisposeIfCreated();
//    }



//#if UNITY_EDITOR

//    [SerializeField] private bool DEBUG_SyncSettingsWithSO;
//    private float cachedMaxReverbTime;

//    private void OnValidate()
//    {
//        if (DEBUG_SyncSettingsWithSO && audioSpatializerSettingsSO != null)
//        {
//            settings = audioSpatializerSettingsSO.settings;
//        }

//        if (Application.isPlaying)
//        {
//            if (settings.maxReverbTime != cachedMaxReverbTime)
//            {
//                Debug.LogWarning("Changing MaxReverbTime during play mode is NOT supported");
//                settings.maxReverbTime = cachedMaxReverbTime;
//            }
//            if (reverbTime > cachedMaxReverbTime)
//            {
//                Debug.LogWarning("ReverbTime CANT be higher than maxReverbTime");
//                reverbTime = cachedMaxReverbTime;
//            }
//        }
//    }


//    [SerializeField] private Vector3 DEBUG_SoundDir;
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.blue;

//        DEBUG_SoundDir = listenerPosition + soundOffsetToPlayer;

//        Gizmos.DrawWireSphere(DEBUG_SoundDir, 1);
//    }

//#endif
//}
