using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    [Header("Audio Settings:")]
    [Space(6)]
    [SerializeField] private AudioTargetData settings;

    [SerializeField] private float volumeUpdateSpeed = 0.5f;
    [SerializeField] private float lowPassUpdateSpeed = 8500;

    public short Id { get; set; }

    private AudioSource source;
    private AudioSpatializer spatializer;

#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private int totalUpdates;
    [SerializeField] private int updateFPS;
    [SerializeField] private int updateFrameTime;
#endif


    private void Awake()
    {
        source = GetComponent<AudioSource>();
        spatializer = GetComponent<AudioSpatializer>();
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer's data
    /// </summary>
    public void UpdateAudioSource(AudioTargetData newSettings)
    {
        settings = newSettings;

        //0 = 100% muffled audio
        spatializer.muffleStrength = newSettings.muffle;

#if UNITY_EDITOR
        totalUpdates += 1;
        updateFPS = (int)math.floor(totalUpdates / Time.time);
        updateFrameTime = (int)math.floor(1f / updateFPS * 1000);
#endif
    }

    public float3 direction;
}
