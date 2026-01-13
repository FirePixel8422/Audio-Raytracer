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

    private static NativeArray<bool> usedIds;

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

        usedIds = new NativeArray<bool>(startCapacity, Allocator.Persistent);

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
        target.AddToAudioSystem(AudioTargetPositions, AllocateId());
    }

    /// <summary>
    /// Get a short id based on a list that tracks used and free ids.
    /// </summary>
    private static short AllocateId()
    {
        short idCount = (short)usedIds.Length;

        for (short i = 0; i < idCount; i++)
        {
            // Check for first free Id
            if (usedIds[i] == false)
            {
                usedIds[i] = true;
                return i;
            }
        }

        // Resize array if we ran out of ids
        NativeArray<bool> old = usedIds;
        usedIds = new NativeArray<bool>(idCount * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        NativeArray<bool>.Copy(old, usedIds, old.Length);
        old.Dispose();

        usedIds[idCount] = true;
        return idCount;
    }
    private static void FreeId(short id)
    {
        usedIds[id] = false;
    }

    public static void RemoveAudioTargetFromSystem(AudioTargetRT target)
    {
        if (target == null || audioTargets.Count == 0) return;

        short removeIndex = target.Id;
        int lastIndex = audioTargets.Count - 1;

        if (removeIndex != lastIndex)
        {
            AudioTargetRT swapped = audioTargets[lastIndex];

            audioTargets[removeIndex] = swapped;
            AudioTargetSettings.NextBatch[removeIndex] = AudioTargetSettings.NextBatch[lastIndex];
            AudioTargetPositions.NextBatch[removeIndex] = AudioTargetPositions.NextBatch[lastIndex];

            usedIds[swapped.Id] = false;
            usedIds[removeIndex] = true;

            swapped.Id = removeIndex;
        }
        else
        {
            FreeId(0);
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
        if (muffleRayHitsCapacity > MuffleRayHits.Length)
        {
            MuffleRayHits.Dispose();
            MuffleRayHits = new NativeArray<ushort>(muffleRayHitsCapacity * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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

        usedIds.Dispose();
        AudioTargetPositions.Dispose();
        AudioTargetSettings.Dispose();
        PermeationStrengthRemains.Dispose();
        MuffleRayHits.Dispose();
    }
}
