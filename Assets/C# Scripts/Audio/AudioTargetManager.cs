using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Mathematics;
using CrowSupport.Events;


[Serializable]
public class AudioTargetManager : MonoBehaviour
{
    [SerializeField] private GameEventSO onAudioTargetUpdateEvent;

    [SerializeField] private int startCapacity = 5;

    private List<AudioTargetRT> audioTargets;
    public int AudioTargetCount_JobBatch => math.min(AudioTargetSettings.JobBatch.Length, audioTargets.Count);
    public int AudioTargetCount_NextBatch => math.min(AudioTargetSettings.NextBatch.Length, audioTargets.Count);

    private NativeIdPool idPool;

    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    // possibly get rid of NativeJobBatch wrapper > Just an array instead???
    public NativeJobBatch<AudioTargetRTSettings> AudioTargetSettings { get; private set; }
    public NativeJobBatch<float3> AudioTargetPositions { get; private set; }
    public NativeArray<ushort> MuffleRayHits { get; private set; }
    public NativeArray<float> PermeationPowerRemains { get; private set; }



    public void Init()
    {
        audioTargets = new List<AudioTargetRT>(startCapacity);

        idPool = new NativeIdPool(startCapacity, Allocator.Persistent);

        AudioTargetSettings = new NativeJobBatch<AudioTargetRTSettings>(startCapacity, Allocator.Persistent);
        AudioTargetPositions = new NativeJobBatch<float3>(startCapacity, Allocator.Persistent);

        MuffleRayHits = new NativeArray<ushort>(startCapacity * AudioRaytracingManager.Instance.ToUseThreadCount, Allocator.Persistent);
        PermeationPowerRemains = new NativeArray<float>(startCapacity * AudioRaytracingManager.Instance.ToUseThreadCount, Allocator.Persistent);
    }
    public void Dispose()
    {
        idPool.Dispose();
        AudioTargetPositions.Dispose();
        AudioTargetSettings.Dispose();
        PermeationPowerRemains.Dispose();
        MuffleRayHits.Dispose();
    }

    #region Add/Remove/Update AudioTargetRT in system

    public void HandleAudioTargetChange((AudioTargetRT Target, AudioTargetChangeType ChangeType) change)
    {
        switch (change.ChangeType)
        {
            case AudioTargetChangeType.Add:
                AddAudioTargetToSystem(change.Target);
                break;

            case AudioTargetChangeType.Update:
                UpdateAudiotargetInSystem(change.Target);
                break;

            case AudioTargetChangeType.Remove:
                RemoveAudioTargetFromSystem(change.Target);
                break;
        }
    }

    private void AddAudioTargetToSystem(AudioTargetRT target)
    {
        audioTargets.Add(target);

        AudioTargetSettings.Add(new AudioTargetRTSettings());

        short audioTargetId = idPool.RequestId();
        target.AddToAudioSystem(AudioTargetPositions, audioTargetId);
    }
    private void UpdateAudiotargetInSystem(AudioTargetRT target)
    {
        target.UpdateToAudioSystem(AudioTargetPositions);
    }
    private void RemoveAudioTargetFromSystem(AudioTargetRT target)
    {
        if (target == null || audioTargets.Count == 0) return;

        short removeIndex = target.Id.Value;
        short lastIndex = (short)(audioTargets.Count - 1);

        // If an AudioTargetRT in the middle of the list is removed, swap it with the last one and swap relevant data
        if (removeIndex != lastIndex)
        {
            AudioTargetRT swapped = audioTargets[lastIndex];

            audioTargets[removeIndex] = swapped;
            AudioTargetSettings.NextBatch[removeIndex] = AudioTargetSettings.NextBatch[lastIndex];
            AudioTargetPositions.NextBatch[removeIndex] = AudioTargetPositions.NextBatch[lastIndex];

            idPool.ReleaseId(removeIndex);
            idPool.SwapIds(removeIndex, swapped.Id.Value);

            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id
            // Its now possible to just use idPool.Length to get the latest id

            swapped.Id.Value = removeIndex;
        }
        else
        {
            idPool.ReleaseId(removeIndex);
        }

        audioTargets.RemoveAt(lastIndex);
        AudioTargetSettings.RemoveLastEntry();
        AudioTargetPositions.RemoveLastEntry();
    }

    #endregion


    public void UpdateJobBatch()
    {
        onAudioTargetUpdateEvent?.Invoke();

        AudioTargetPositions.UpdateJobBatch();
        AudioTargetSettings.UpdateJobBatch();

        int maxBatchCapacity = audioTargets.Count * AudioRaytracingManager.Instance.ToUseThreadCount;

        // Resize MuffleRayHits array if needed
        if (maxBatchCapacity != MuffleRayHits.Length)
        {
            MuffleRayHits.Dispose();
            MuffleRayHits = new NativeArray<ushort>(maxBatchCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            PermeationPowerRemains.Dispose();
            PermeationPowerRemains = new NativeArray<float>(maxBatchCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
    }
    public void UpdateAudioTargetSettings()
    {
        // Update audio targets
        for (short audioTargetId = 0; audioTargetId < AudioTargetCount_JobBatch; audioTargetId++)
        {
            AudioTargetRTSettings settings = AudioTargetSettings.JobBatch[audioTargetId];

            audioTargets[audioTargetId].UpdateAudioSource(settings);
        }
    }
}
