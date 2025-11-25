using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


public class AudioRayTracer : MonoBehaviour
{
    [SerializeField] private float3 rayOrigin;

    [Range(1, 15000)]
    [SerializeField] int rayCount = 1000;

    [Range(0, 25)]
    [SerializeField] int maxBounces = 3;

    [Range(0, 1000)]
    [SerializeField] float maxRayDist = 10;

    //[SerializeField] private NativeSampledAnimationCurve muffleFalloffCurve;

    private NativeArray<half3> rayDirections;
    private NativeArray<half3> echoRayDirections;

    private NativeArray<AudioRayResult> rayResults;
    private NativeArray<byte> rayResultCounts;



    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    private void Awake()
    {
        InitializeAudioRaytraceSystem();

#if UNITY_EDITOR
        sw = new System.Diagnostics.Stopwatch();
        sw2 = new System.Diagnostics.Stopwatch();
#endif
    }


    #region Setup Raytrace System and data Methods

    private void InitializeAudioRaytraceSystem()
    {
        //initialize Raycast native arrays
        rayDirections = new NativeArray<half3>(rayCount, Allocator.Persistent);

        //generate sphere directions with fibonacci sphere algorithm
        var generateDirectionsJob = new FibonacciDirectionsJobParallel
        {
            directions = rayDirections,
        };

        JobHandle mainJobHandle = generateDirectionsJob.Schedule(rayCount, 512);

        // Do all other tasks here to give the sphere direcion job some time to complete before forcing it to complete.
        int maxRayResultsArrayLength = rayCount * (maxBounces + 1);

        rayResults = new NativeArray<AudioRayResult>(maxRayResultsArrayLength, Allocator.Persistent);
        rayResultCounts = new NativeArray<byte>(rayCount, Allocator.Persistent);
        echoRayDirections = new NativeArray<half3>(maxRayResultsArrayLength, Allocator.Persistent);

        mainJobHandle.Complete();
    }

    #endregion


    private AudioRayTracerJobParallelBatchedOld audioRayTraceJob;
    private ProcessAudioDataJob calculateAudioTargetDataJob;
    private JobHandle mainJobHandle;

    private void OnUpdate()
    {
        //if computeAsync is true skip a frame if job is not done yet
        if (AudioRaytracersManager.ComputeAsync && mainJobHandle.IsCompleted == false) return;
        
        mainJobHandle.Complete();

#if UNITY_EDITOR
        sw2.Restart();
#endif

        // Trigger an update for all audio targets with ray traced data after raytrace job has finished
        AudioTargetManager.UpdateAudioTargetSettings();

        AudioTargetManager.CycleToNextBatch();
        AudioColliderManager.CycleToNextBatch();

#if UNITY_EDITOR
        batchCycleMs = sw2.ElapsedMilliseconds;
#endif


#if UNITY_EDITOR

        raytracerMs = sw.ElapsedMilliseconds;
        sw.Restart();

        //failsafe to prevent crash when updating maxBounces in editor
        if (audioRayTraceJob.RayDirections.Length != 0 && (audioRayTraceJob.MaxRayHits != (maxBounces + 1) || rayDirections.Length != rayCount))
        {
            //recreate rayResults and echoRayDirections arrays with new size because maxBounces or rayCount changed
            rayResults = new NativeArray<AudioRayResult>(rayCount * (maxBounces + 1), Allocator.Persistent);
            echoRayDirections = new NativeArray<half3>(rayCount * (maxBounces + 1), Allocator.Persistent);

            if (rayDirections.Length != rayCount)
            {
                //reculcate ray directions and resize rayResultCounts if rayCount changed
                rayDirections = new NativeArray<half3>(rayCount, Allocator.Persistent);
                rayResultCounts = new NativeArray<byte>(rayCount, Allocator.Persistent);

                var generateDirectionsJob = new FibonacciDirectionsJobParallel
                {
                    directions = rayDirections
                };

                generateDirectionsJob.Schedule(rayCount, 64).Complete();

                Debug.LogWarning("You changed the rayCount in the inspector. This will cause a crash in Builds, failsafe triggered: Recreated rayDirections array with new capacity.");
            }

            Debug.LogWarning("You changed the max bounces/rayCount in the inspector. This will cause a crash in Builds, failsafe triggered: Recreated rayResults array with new capacity.");
        }

        if (rayResults.IsCreated && (drawRayHitsGizmos || drawRayTrailsGizmos))
        {
            DEBUG_rayResults = rayResults.ToArray();
            DEBUG_rayResultCounts = rayResultCounts.ToArray();

            DEBUG_echoRayDirections = echoRayDirections.ToArray();
            DEBUG_muffleRayHits = AudioTargetManager.MuffleRayHits.CurrentBatch.AsArray().ToArray();
        }
#endif


        #region Raycasting Job ParallelBatched

        // Create raytrace job and fire it
        audioRayTraceJob = new AudioRayTracerJobParallelBatchedOld
        {
            RayOrigin = (float3)transform.position + rayOrigin,
            RayDirections = rayDirections,

            AABBColliders = AudioColliderManager.AABBColliders.CurrentBatch.AsArray(),
            AABBCount = AudioColliderManager.AABBColliders.CurrentBatch.Length,

            OBBColliders = AudioColliderManager.OBBColliders.CurrentBatch.AsArray(),
            OBBCount = AudioColliderManager.OBBColliders.CurrentBatch.Length,

            SphereColliders = AudioColliderManager.SphereColliders.CurrentBatch.AsArray(),
            SphereCount = AudioColliderManager.SphereColliders.CurrentBatch.Length,

            AudioTargetPositions = AudioTargetManager.AudioTargetPositions.CurrentBatch.AsArray(),

            MaxRayHits = maxBounces + 1,
            MaxRayDist = maxRayDist,
            TotalAudioTargets = AudioTargetManager.AudioTargetCount,

            Results = rayResults,
            ResultCounts = rayResultCounts,

            EchoRayDirections = echoRayDirections,
            
            MuffleRayHits = AudioTargetManager.MuffleRayHits.CurrentBatch.AsArray(),
        };

        int batchSize = (int)math.max(1, math.ceil((float)rayCount / AudioRaytracersManager.ToUseThreadCount));

        mainJobHandle = audioRayTraceJob.Schedule(rayCount, batchSize);

        #endregion


        #region Calculate Audio Target Data Job

        float3 listenerRight = math.normalize(transform.right); // Player Cam Right direction (for panning)

        calculateAudioTargetDataJob = new ProcessAudioDataJob
        {
            RayResults = rayResults,
            RayResultCounts = rayResultCounts,

            EchoRayDirections = echoRayDirections,

            TotalAudioTargets = AudioTargetManager.AudioTargetCount,
            AudioTargetPositions = AudioTargetManager.AudioTargetPositions.CurrentBatch.AsArray(),
            AudioTargetRTData = AudioTargetManager.AudioTargetRTData.CurrentBatch.AsArray(),

            MuffleRayHits = AudioTargetManager.MuffleRayHits.CurrentBatch.AsArray(),

            MaxRayHits = maxBounces + 1,
            RayCount = rayCount,
            RayOriginWorld = (float3)transform.position + rayOrigin,

            ListenerRightDir = listenerRight,
        };

        //start job and give mainJobHandle dependency, so it only start after the raytrace job is done.
        //update mainJobHandle to include this new job for its completion signal
        mainJobHandle = JobHandle.CombineDependencies(mainJobHandle, calculateAudioTargetDataJob.Schedule(mainJobHandle));

        #endregion
    }



    private void OnDestroy()
    {
        // Force complete all jobs
        mainJobHandle.Complete();

        // Ray arrays
        rayDirections.DisposeIfCreated();
        rayResults.DisposeIfCreated();
        rayResultCounts.DisposeIfCreated();

        // Audio arrays
        echoRayDirections.DisposeIfCreated();
    }




#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField] private bool drawRayHitsGizmos = true;
    [SerializeField] private bool drawRayTrailsGizmos;
    [SerializeField] private bool drawReturnRayDirectionGizmos;
    [SerializeField] private bool drawReturnRayLastDirectionGizmos;
    [SerializeField] private bool drawReturnRaysAvgDirectionGizmos;


    [SerializeField] private Color originColor = Color.green;

    [SerializeField] private Color rayHitColor = Color.cyan;
    [SerializeField] private Color rayTrailColor = new Color(0, 1, 0, 0.15f);

    [SerializeField] private Color rayReturnDirectionColor = new Color(0.5f, 0.25f, 0, 1f);
    [SerializeField] private Color rayReturnLastDirectionColor = new Color(1, 0.5f, 0, 1);
    [SerializeField] private Color rayReturnAvgDirectionColor = new Color(1, 0.5f, 0, 1);


    private AudioRayResult[] DEBUG_rayResults;
    private byte[] DEBUG_rayResultCounts;

    private half3[] DEBUG_echoRayDirections;

    [SerializeField] private ushort[] DEBUG_muffleRayHits;

    [SerializeField] private uint[] hashes; 

    [SerializeField] private float raytracerMs;
    [SerializeField] private float batchCycleMs;
    private System.Diagnostics.Stopwatch sw;
    private System.Diagnostics.Stopwatch sw2;


    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;

        float3 rayOrigin = (float3)transform.position + this.rayOrigin;


        if (DEBUG_rayResults != null && DEBUG_rayResults.Length != 0 && (drawRayHitsGizmos || drawRayTrailsGizmos || drawReturnRayDirectionGizmos || drawReturnRaysAvgDirectionGizmos))
        {
            AudioRayResult prevResult = AudioRayResult.Null;

            int maxRayHits = DEBUG_rayResults.Length / DEBUG_rayResultCounts.Length;

            int setResultAmountsCount = DEBUG_rayResultCounts.Length;

            int cSetResultCount;

            float3 returningRayDir;
            float3 lastReturningRayOrigin;

            float3 lastReturningRayOriginTotal = float3.zero;
            int lastReturningRayOriginsCount = 0;

            if (setResultAmountsCount * maxRayHits > 5000)
            {
                Debug.LogWarning("Max Gizmos Reached (5k) please turn of gizmos to not fry CPU");

                setResultAmountsCount = 5000 / maxRayHits;
            }

            for (int i = 0; i < setResultAmountsCount; i++)
            {
                cSetResultCount = DEBUG_rayResultCounts[i];
                prevResult.DEBUG_HitPoint = (half3)rayOrigin;


                //ray hit markers and trails
                for (int i2 = 0; i2 < cSetResultCount; i2++)
                {
                    AudioRayResult result = DEBUG_rayResults[i * maxRayHits + i2];

                    Gizmos.color = rayHitColor;

                    if (drawRayHitsGizmos)
                    {
                        Gizmos.DrawWireCube((float3)result.DEBUG_HitPoint, Vector3.one * 0.1f);
                    }

                    Gizmos.color = rayTrailColor;

                    if (drawRayTrailsGizmos)
                    {
                        Gizmos.DrawLine((float3)prevResult.DEBUG_HitPoint, (float3)result.DEBUG_HitPoint);
                        prevResult = result;
                    }
                }


                lastReturningRayOrigin = float3.zero;

                //return to origin rays of each ray and avg direction of all rays last visible player ray
                if (drawReturnRayDirectionGizmos || drawReturnRaysAvgDirectionGizmos)
                {
                    Gizmos.color = rayReturnDirectionColor;

                    //get all ray origins that returned to the original origin and save last ray that did this
                    for (int i2 = 0; i2 < maxRayHits; i2++)
                    {
                        returningRayDir = DEBUG_echoRayDirections[i * maxRayHits + i2];

                        if (cSetResultCount != 0 && DEBUG_rayResults[i * maxRayHits + cSetResultCount - 1].AudioTargetId == 0 && math.distance(returningRayDir, float3.zero) != 0)
                        {
                            lastReturningRayOrigin = (float3)DEBUG_rayResults[i * maxRayHits + i2].DEBUG_HitPoint / (DEBUG_rayResults[i * maxRayHits + i2].FullRayDistance != 0 ? DEBUG_rayResults[i * maxRayHits + i2].FullRayDistance : 1) * 125 / 2;

                            if (drawReturnRayDirectionGizmos)
                            {
                                Gizmos.color = rayReturnDirectionColor;
                                Gizmos.DrawLine(rayOrigin, lastReturningRayOrigin);
                            }
                        }
                    }

                    //draw last ray that returned to origin and add its origin to lastReturningRayOriginTotal
                    if (math.distance(lastReturningRayOrigin, float3.zero) != 0)
                    {
                        lastReturningRayOriginTotal += lastReturningRayOrigin;
                        lastReturningRayOriginsCount++;

                        if (drawReturnRayLastDirectionGizmos)
                        {
                            Gizmos.color = rayReturnLastDirectionColor;
                            Gizmos.DrawLine(rayOrigin, lastReturningRayOrigin);
                        }
                    }
                }
            }

            //avg direction of all rays based on lastReturningRayOriginTotal divided by amount of ray origins added here (equal to amount of last rays that returned to origin)
            if (drawReturnRaysAvgDirectionGizmos)
            {
                Gizmos.color = rayReturnAvgDirectionColor;
                Gizmos.DrawLine(rayOrigin, rayOrigin + math.normalize(lastReturningRayOriginTotal / lastReturningRayOriginsCount - rayOrigin) * 2);
            }
        }

        //origin cube
        Gizmos.color = originColor;
        Gizmos.DrawWireSphere(rayOrigin, 0.025f);
        Gizmos.DrawWireSphere(rayOrigin, 0.05f);
    }
#endif
}
