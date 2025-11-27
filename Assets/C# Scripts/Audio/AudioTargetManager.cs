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
    public static int AudioTargetCount => math.min(AudioTargetRTData.CurrentBatchLength, audioTargets.Count);

    private static NativeList<bool> usedIds;

    public static NativeListBatch<AudioTargetRTData> AudioTargetRTData { get; private set; }
    public static NativeListBatch<float3> AudioTargetPositions { get; private set; }
    public static NativeListBatch<ushort> MuffleRayHits { get; private set; }

    public static Action OnAudioTargetUpdate { get; set; }


    private void Awake()
    {
        audioTargets = new List<AudioTargetRT>(startCapacity);

        usedIds = new NativeList<bool>(startCapacity, Allocator.Persistent);

        AudioTargetRTData = new NativeListBatch<AudioTargetRTData>(startCapacity, Allocator.Persistent);
        AudioTargetPositions = new NativeListBatch<float3>(startCapacity, Allocator.Persistent);

        MuffleRayHits = new NativeListBatch<ushort>(startCapacity, Allocator.Persistent);
        MuffleRayHits.NextBatch.Length = startCapacity;
    }


    #region Add/Remove/Update AudioTargetRT in system

    public static void AddAudioTargetToSystem(AudioTargetRT target)
    {
        audioTargets.Add(target);

        AudioTargetRTData.Add(new AudioTargetRTData());
        target.AddToAudioSystem(AudioTargetPositions, AllocateId());

        int muffleRayHitsCapacity = AudioTargetCount * AudioRaytracersManager.ToUseThreadCount;

        // Resize MuffleRayHits array if needed
        if (muffleRayHitsCapacity > MuffleRayHits.NextBatch.Capacity)
        {
            MuffleRayHits.NextBatch.Resize(muffleRayHitsCapacity, NativeArrayOptions.UninitializedMemory);
            MuffleRayHits.NextBatch.Length = muffleRayHitsCapacity;
        }
    }
    private static short AllocateId()
    {
        for (short i = 0; i < usedIds.Length; i++)
        {
            if (!usedIds[i])
            {
                usedIds[i] = true;
                return i;
            }
        }
        short newId = (short)usedIds.Length;

        usedIds.Add(true);

        return newId;
    }
    private static void FreeId(short id)
    {
        usedIds[id] = false;
    }

    public static void RemoveAudioTargetFromSystem(AudioTargetRT target)
    {
        if (target == null) return;

        short targetId = target.Id;

        // Validate the ID
        if (targetId < 0 || targetId >= audioTargets.Count)
        {
            DebugLogger.LogWarning($"Tried to remove AudioTarget with invalid Id {targetId}");
            return;
        }

        int lastIndex = audioTargets.Count - 1;

        // Remove elements from lists
        audioTargets.RemoveAtSwapBack(targetId);
        AudioTargetRTData.RemoveAtSwapBack(targetId);
        AudioTargetPositions.RemoveAtSwapBack(targetId);

        // Only update swapped element if we actually swapped
        if (targetId != lastIndex)
        {
            audioTargets[targetId].Id = targetId;
        }

        FreeId(targetId);
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

        usedIds.Dispose();
        AudioTargetPositions.Dispose();
        AudioTargetRTData.Dispose();
        MuffleRayHits.Dispose();
    }
}
