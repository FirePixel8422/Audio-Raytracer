using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioReverbFilter))]
public class AudioTargetRT : MonoBehaviour
{
    [Header("Audio Settings:")]
    [Space(6)]
    [SerializeField] private AudioTargetData settings;
    [SerializeField] private float baseVolume;

    [SerializeField] private float volumeUpdateSpeed = 0.5f;
    [SerializeField] private float lowPassUpdateSpeed = 8500;

    public short Id { get; set; }

    private AudioSource source;
    private AudioSpatializer spatializer;
    private AudioReverbFilter reverb;


    private void Awake()
    {
        source = GetComponent<AudioSource>();
        spatializer = GetComponent<AudioSpatializer>();
        reverb = GetComponent<AudioReverbFilter>();

        baseVolume = source.volume;
        settings.muffle = baseVolume;
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer's data
    /// </summary>
    public void UpdateAudioSource(AudioTargetData newSettings)
    {
        settings = newSettings;

        //0 = 100% muffled audio
        //spatializer.UpdateSettings(newSettings);
        spatializer.muffleStrength = newSettings.muffle;
    }

    public float3 direction;
}
