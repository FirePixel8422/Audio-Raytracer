using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Mathematics;
using Fire_Pixel.Utility;


public class AudioTargetManager : MonoBehaviour
{
    [Header("Start capacity of audioTarget arrays")]
    [SerializeField] private int startCapacity = 5;

    private static List<AudioTargetRT> audioTargets;
    public static int AudioTargetCount_JobBatch => math.min(AudioTargetSettings.JobBatch.Length, audioTargets.Count);
    public static int AudioTargetCount_NextBatch => math.min(AudioTargetSettings.NextBatch.Length, audioTargets.Count);

    private static NativeIdPool idPool;

    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    public static NativeJobBatch<AudioTargetRTSettings> AudioTargetSettings { get; private set; }
    public static NativeJobBatch<float3> AudioTargetPositions { get; private set; }
    public static NativeArray<ushort> MuffleRayHits { get; private set; }
    public static NativeArray<half> PermeationStrengthRemains { get; private set; }

    public static Action OnAudioTargetUpdate { get; set; }


    private void Awake()
    {
        audioTargets = new List<AudioTargetRT>(startCapacity);

        idPool = new NativeIdPool(startCapacity, Allocator.Persistent);

        AudioTargetSettings = new NativeJobBatch<AudioTargetRTSettings>(startCapacity, Allocator.Persistent);
        AudioTargetPositions = new NativeJobBatch<float3>(startCapacity, Allocator.Persistent);

        MuffleRayHits = new NativeArray<ushort>(startCapacity * AudioRaytracersManager.ToUseThreadCount, Allocator.Persistent);
        PermeationStrengthRemains = new NativeArray<half>(startCapacity * AudioRaytracersManager.ToUseThreadCount, Allocator.Persistent);
    }


    #region Add/Remove/Update AudioTargetRT in system

    public static void AddAudioTargetToSystem(AudioTargetRT target)
    {
        audioTargets.Add(target);

        AudioTargetSettings.Add(new AudioTargetRTSettings());

        short audioTargetId = idPool.RequestId();
        target.AddToAudioSystem(AudioTargetPositions, audioTargetId);
    }

    public static void RemoveAudioTargetFromSystem(AudioTargetRT target)
    {
        if (target == null || audioTargets.Count == 0) return;

        short removeIndex = target.Id;
        short lastIndex = (short)(audioTargets.Count - 1);

        // If an AudioTargetRT in the middle of the list is removed, swap it with the last one and swap relevant data
        if (removeIndex != lastIndex)
        {
            AudioTargetRT swapped = audioTargets[lastIndex];

            audioTargets[removeIndex] = swapped;
            AudioTargetSettings.NextBatch[removeIndex] = AudioTargetSettings.NextBatch[lastIndex];
            AudioTargetPositions.NextBatch[removeIndex] = AudioTargetPositions.NextBatch[lastIndex];

            idPool.ReleaseId(removeIndex);
            idPool.SwapIds(removeIndex, swapped.Id);

            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id

            swapped.Id = removeIndex;
        }
        else
        {
            idPool.ReleaseId(removeIndex);
        }

        audioTargets.RemoveAt(lastIndex);
        AudioTargetSettings.NextBatch.RemoveAt(lastIndex);
        AudioTargetPositions.NextBatch.RemoveAt(lastIndex);
    }
    public static void UpdateColiderInSystem(AudioTargetRT target)
    {
        target.UpdateToAudioSystem(AudioTargetPositions);
    }

    #endregion


    public static void UpdateJobBatch()
    {
        OnAudioTargetUpdate?.Invoke();

        AudioTargetPositions.UpdateJobBatch();
        AudioTargetSettings.UpdateJobBatch();

        int muffleRayHitsCapacity = audioTargets.Count * AudioRaytracersManager.ToUseThreadCount;

        // Resize MuffleRayHits array if needed
        if (muffleRayHitsCapacity != MuffleRayHits.Length)
        {
            MuffleRayHits.Dispose();
            MuffleRayHits = new NativeArray<ushort>(muffleRayHitsCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
    }
    public static void UpdateAudioTargetSettings()
    {
        // Update audio targets
        for (short audioTargetId = 0; audioTargetId < AudioTargetCount_JobBatch; audioTargetId++)
        {
            AudioTargetRTSettings settings = AudioTargetSettings.JobBatch[audioTargetId];

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

        idPool.Dispose();
        AudioTargetPositions.Dispose();
        AudioTargetSettings.Dispose();
        PermeationStrengthRemains.Dispose();
        MuffleRayHits.Dispose();
    }

    [SerializeField] private byte[] DEBUG_idPool;
    private void Update()
    {
        DEBUG_idPool = idPool.IdList.ToArray();
    }
}
