using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    public short Id { get; set; }

    private AudioSpatializer spatializer;

#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private int totalUpdates;
    [SerializeField] private int updateFPS;
    [SerializeField] private int updateFrameTime;
#endif


    private void Awake()
    {
        spatializer = GetComponent<AudioSpatializer>();
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer's data
    /// </summary>
    public void UpdateAudioSource(AudioTargetData newSettings)
    {
        spatializer.MuffleStrength = newSettings.muffle;


#if UNITY_EDITOR
        totalUpdates += 1;
        updateFPS = (int)math.floor(totalUpdates / Time.time);
        updateFrameTime = (int)math.floor(1f / updateFPS * 1000);
#endif
    }
}
