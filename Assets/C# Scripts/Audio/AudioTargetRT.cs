using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter), typeof(AudioHighPassFilter), typeof(AudioReverbFilter))]
public class AudioTargetRT : MonoBehaviour
{
    [Header("Audio Settings:")]
    [Space(6)]
    [SerializeField] private AudioSettings settings;
    [SerializeField] private float baseVolume;

    [SerializeField] private float volumeUpdateSpeed = 0.5f;
    [SerializeField] private float lowPassUpdateSpeed = 8500;

    public short Id { get; set; }

    private AudioSource source;
    private AudioSpatializer spatializer;
    private AudioReverbFilter reverb;



    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);


    private void Start()
    {
        source = GetComponent<AudioSource>();
        spatializer = GetComponent<AudioSpatializer>();
        reverb = GetComponent<AudioReverbFilter>();

        baseVolume = source.volume;
        settings.volume = baseVolume;

        spatializer.muffleStrength = 0f;
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer's data
    /// </summary>
    /// <param name="audioStrength">float between 0 and 1 equal to percent of rays that hit this audiotarget</param>
    /// <param name="panStereo">what pan stereo value (-1, 1) direction the audio came from</param>
    /// <param name="mufflePercentage">float between 0 and 1 equal to how muffled the sound should be, 0 is 100% muffled</param>
    public void UpdateAudioSource(AudioSettings newSettings)
    {
        newSettings.volume = baseVolume;

        //DEBUG






        settings = newSettings;

        //0 = 100% muffled audio
        spatializer.muffleStrength = 1 - newSettings.muffle;





        //DEBUG
        //source.panStereo = newSettings.panStereo;
    }

    public float3 direction;



    private void OnUpdate()
    {
        float deltaTime = Time.deltaTime;

        //maybe make this method smarter, make it so it takes MAX volumeUpdatepeed to change from a to b

       // source.volume = MathLogic.MoveTowards(source.volume, settings.volume, volumeUpdateSpeed * deltaTime);
    }
}
