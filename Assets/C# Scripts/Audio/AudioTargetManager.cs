using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Mathematics;


public class AudioTargetManager : MonoBehaviour
{
    [Header("Start capacity of audioTarget arrays")]
    [SerializeField] private int startCapacity = 5;

    private static List<AudioTargetRT> audioTargets;
    public static int AudioTargetCount => audioTargets.Count;

    public static NativeListBatch<AudioTargetRTData> AudioTargetRTData { get; private set; }
    public static NativeListBatch<float3> AudioTargetPositions { get; private set; }
    public static NativeListBatch<ushort> MuffleRayHits { get; private set; }

    public static Action OnAudioTargetUpdate { get; set; }


    private void Awake()
    {
        audioTargets = new List<AudioTargetRT>(startCapacity);

        AudioTargetRTData = new NativeListBatch<AudioTargetRTData>(startCapacity, Allocator.Persistent);
        AudioTargetPositions = new NativeListBatch<float3>(startCapacity, Allocator.Persistent);
        MuffleRayHits = new NativeListBatch<ushort>(startCapacity, Allocator.Persistent);
    }


    #region Add/Remove/Update AudioTargetRT in system

    public static void AddAudioTargetToSystem(AudioTargetRT target)
    {
        audioTargets.Add(target);

        target.AddToAudioSystem(AudioTargetPositions);

        // Resize MuffleRayHits array if needed
        if (AudioTargetCount * AudioRaytracersManager.ToUseThreadCount > MuffleRayHits.NextBatch.Capacity)
        {
            MuffleRayHits.NextBatch.Resize(AudioTargetCount * AudioRaytracersManager.ToUseThreadCount, NativeArrayOptions.UninitializedMemory);
        }
    }

    public static void RemoveAudioTargetFromSystem(AudioTargetRT target)
    {
        if (target == null) return;

        audioTargets.RemoveAtSwapBack(target.Id);
    }

    public static void UpdateColiderInSystem(AudioTargetRT target)
    {
        target.UpdateToAudioSystem(AudioTargetPositions);
    }

    #endregion


    public static void CycleToNextBatch()
    {
        OnAudioTargetUpdate?.Invoke();

        AudioTargetPositions.CycleToNextBatch();
        AudioTargetRTData.CycleToNextBatch();
        MuffleRayHits.CycleToNextBatch();
    }
    public static void UpdateAudioTargetSettings()
    {
        print(AudioTargetRTData.CurrentBatch.Length);

        // Update audio targets
        for (short audioTargetId = 0; audioTargetId < AudioTargetCount; audioTargetId++)
        {
            AudioTargetSettings settings = AudioTargetRTData.CurrentBatch[audioTargetId].AudioTargetSettings;

            audioTargets[audioTargetId].UpdateAudioSource(settings);
        }
    }

    private void OnDestroy()
    {
        UpdateScheduler.CreateLateOnApplicationQuitCallback(Dispose);
    }

    private void Dispose()
    {
        OnAudioTargetUpdate = null;

        AudioTargetPositions.Dispose();
        AudioTargetRTData.Dispose();
        MuffleRayHits.Dispose();
    }
}
