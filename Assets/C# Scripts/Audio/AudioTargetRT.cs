using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    [SerializeField] private bool IsStatic = true;

    public short Id;

    private AudioSpatializer spatializer;
    private Vector3 lastWorldPosition;


#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private int totalUpdates;
    [SerializeField] private int updateFPS;
    [SerializeField] private int updateFrameTime;
#endif


    private void Awake()
    {
        spatializer = GetComponent<AudioSpatializer>();
        lastWorldPosition = transform.position;
    }


    private void OnEnable() => AudioTargetManager.AddAudioTargetToSystem(this);
    private void OnDisable() => AudioTargetManager.RemoveAudioTargetFromSystem(this);

    private void OnDestroy()
    {
        if (IsStatic == false)
        {
            AudioTargetManager.OnAudioTargetUpdate -= CheckTransformation;
        }
    }

    public void AddToAudioSystem(NativeListBatch<float3> audioTargetPositions)
    {
        audioTargetPositions.Add(transform.position);
    }
    public void UpdateToAudioSystem(NativeListBatch<float3> audioTargetPositions)
    {
        audioTargetPositions.Set(Id, transform.position);
    }

    private void CheckTransformation()
    {
        if (transform.position != lastWorldPosition)
        {
            AudioTargetManager.UpdateColiderInSystem(this);
        }
        lastWorldPosition = transform.position;
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer's data
    /// </summary>
    public void UpdateAudioSource(AudioTargetSettings newSettings)
    {
        spatializer.MuffleStrength = newSettings.muffle;


#if UNITY_EDITOR
        totalUpdates += 1;
        updateFPS = (int)math.floor(totalUpdates / Time.time);
        updateFrameTime = (int)math.floor(1f / updateFPS * 1000);
#endif
    }
}
