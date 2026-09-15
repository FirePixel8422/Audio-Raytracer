using CrowSupport.Events;
using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    [SerializeField] private AudioTargetRTEventSO onUpdateAudioTargetRTEvent;
    [SerializeField] private GameEventSO onAudioTargetUpdateEvent;

    [Tooltip("Set this to true if audioTarget never moves at runtime")]
    [SerializeField] private bool isStatic = true;
    public bool IsStatic => isStatic;

    public UpdateValue<short> Id;

    private AudioSpatializer spatializer;
    private Vector3 lastWorldPosition;


    private void Awake()
    {
        spatializer = GetComponent<AudioSpatializer>();
        lastWorldPosition = transform.position;

        if (IsStatic == false)
        {
            onAudioTargetUpdateEvent += CheckTransformation;
        }
    }

    private void OnEnable() => onUpdateAudioTargetRTEvent?.Invoke((this, AudioTargetChangeType.Add));
    private void OnDisable() => onUpdateAudioTargetRTEvent?.Invoke((this, AudioTargetChangeType.Remove));

    private void OnDestroy()
    {
        if (IsStatic) return;

        onAudioTargetUpdateEvent -= CheckTransformation;
    }

    public void AddToAudioSystem(NativeJobBatch<float3> audioTargetPositions, short assignedId)
    {
        audioTargetPositions.Add(transform.position);
        Id.Value = assignedId;
    }
    public void UpdateToAudioSystem(NativeJobBatch<float3> audioTargetPositions)
    {
        DebugLogger.Log(Id.Value);
        audioTargetPositions[Id.Value] = transform.position;
    }

    /// <summary>
    /// Update AudioTarget position in the audio system if it has moved
    /// </summary>
    private void CheckTransformation()
    {
        if (enabled == false) return;

        if (transform.position != lastWorldPosition)
        {
            onUpdateAudioTargetRTEvent?.Invoke((this, AudioTargetChangeType.Update));
        }
        lastWorldPosition = transform.position;
    }


    /// <summary>
    /// Update AudioTarget at realtime based on the AudioRaytracer job results
    /// </summary>
    public void UpdateAudioSource(AudioTargetRTSettings newSettings)
    {
        spatializer.UpdateSpatializer(newSettings);
    }


#if UNITY_EDITOR
    private bool prevIsStatic;
    public void SetStaticState(bool value)
    {
        isStatic = value;
        prevIsStatic = value;
    }

    // Enforce equal staticness on all attached colliders and AudioTargetRT on the same gameobject
    private void OnValidate()
    {
        if (prevIsStatic != IsStatic)
        {
            if (Application.isPlaying) return;

            AudioCollider[] audioColliders = GetComponents<AudioCollider>();
            for (int i = 0; i < audioColliders.Length; i++)
            {
                audioColliders[i].SetIsStaticValue(IsStatic);
            }

            DebugLogger.Log($"Possible AudioTarget and all attached AudioColliders set to the same static value", audioColliders.Length != 0);
        }
        prevIsStatic = IsStatic;
    }
#endif
}
