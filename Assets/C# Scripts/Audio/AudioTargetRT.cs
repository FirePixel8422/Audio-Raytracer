using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    [SerializeField] private bool IsStatic = true;

    public short Id;

    private AudioSpatializer spatializer;
    private Vector3 lastWorldPosition;


    private void Awake()
    {
        spatializer = GetComponent<AudioSpatializer>();
        lastWorldPosition = transform.position;

        if (IsStatic == false)
        {
            AudioTargetManager.OnAudioTargetUpdate += CheckTransformation;
        }
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

    public void AddToAudioSystem(NativeJobBatch<float3> audioTargetPositions, short assignedId)
    {
        audioTargetPositions.Add(transform.position);
        Id = assignedId;
    }
    public void UpdateToAudioSystem(NativeJobBatch<float3> audioTargetPositions)
    {
        audioTargetPositions.Set(Id, transform.position);
    }

    private void CheckTransformation()
    {
        if (gameObject.activeInHierarchy == false) return;

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
    }
}
