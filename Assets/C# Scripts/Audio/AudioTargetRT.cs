using System;
using Unity.Mathematics;
using UnityEngine;


public class AudioTargetRT : MonoBehaviour
{
    [Header("Set this to true if audioTarget never moves at runtime")]
    [SerializeField] private bool isStatic = true;
    public bool IsStatic => isStatic;

    public short Id;
    public Action<short> OnIdChanged;

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
        OnIdChanged?.Invoke(assignedId);
    }
    public void UpdateToAudioSystem(NativeJobBatch<float3> audioTargetPositions)
    {
        audioTargetPositions.Set(Id, transform.position);
    }

    private void CheckTransformation()
    {
        if (enabled == false) return;

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


#if UNITY_EDITOR
    private bool prevIsStatic;
    public void SetIsStaticValue(bool value)
    {
        isStatic = value;
        prevIsStatic = value;
    }

    // Enforce equal staticness on all attached colliders and AudioTargetRT on the same gameobject
    private void OnValidate()
    {
        if (prevIsStatic != IsStatic)
        {
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
