using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class AudioRayTracer : UpdateMonoBehaviour
{
    [SerializeField] private float3 rayOrigin;

    [Range(1, 5000)]
    [SerializeField] private int rayCount = 1000;

    [Range(0, 25)]
    [SerializeField] private int maxBounces = 3;
    public int MaxRayHits => maxBounces + 1;

    [Range(0, 500)]
    [SerializeField] private float maxRayLife = 10;

    //[SerializeField] private NativeSampledAnimationCurve muffleFalloffCurve;

    private NativeArray<half3> rayDirections;
    private NativeArray<half3> echoRayDirections;

    private NativeArray<AudioRayResult> rayResults;
    private NativeArray<byte> rayResultCounts;


    private void Awake()
    {
        InitializeAudioRaytraceSystem();

#if UNITY_EDITOR
        raytracerJobsStopwatch = new System.Diagnostics.Stopwatch();
        batchCycleStopwatch = new System.Diagnostics.Stopwatch();
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
        int maxRayResultsArrayLength = rayCount * MaxRayHits;

        rayResults = new NativeArray<AudioRayResult>(maxRayResultsArrayLength, Allocator.Persistent);
        rayResultCounts = new NativeArray<byte>(rayCount, Allocator.Persistent);
        echoRayDirections = new NativeArray<half3>(maxRayResultsArrayLength, Allocator.Persistent);

        mainJobHandle.Complete();
    }

    #endregion


    private AudioRaytracerJobBatched audioRayTraceJob;
    private ProcessAudioDataJob calculateAudioTargetDataJob;
    private JobHandle mainJobHandle;

    protected override void OnUpdate()
    {
        //if computeAsync is true skip a frame if job is not done yet
        if ((AudioRaytracersManager.ComputeAsync && mainJobHandle.IsCompleted == false) || AudioTargetManager.AudioTargetCount_NextBatch == 0) return;
        
        mainJobHandle.Complete();

#if UNITY_EDITOR
        batchCycleStopwatch.Restart();

        raytracerMs = raytracerJobsStopwatch.ElapsedMilliseconds;
        raytracerJobsStopwatch.Restart();
#endif

        // Trigger an update for all audio targets with ray traced data after raytrace job has finished
        AudioTargetManager.UpdateAudioTargetSettings();

#if UNITY_EDITOR
        // Failsafe to prevent crash when updating maxBounces in editor
        if (audioRayTraceJob.RayDirections.Length != 0 && (audioRayTraceJob.MaxRayHits != MaxRayHits || rayDirections.Length != rayCount))
        {
            //recreate rayResults and echoRayDirections arrays with new size because maxBounces or rayCount changed
            rayResults = new NativeArray<AudioRayResult>(rayCount * MaxRayHits, Allocator.Persistent);
            echoRayDirections = new NativeArray<half3>(rayCount * MaxRayHits, Allocator.Persistent);

            if (rayDirections.Length != rayCount)
            {
                //reculcate ray directions and resize rayResultCounts if rayCount changed
                rayDirections = new NativeArray<half3>(rayCount, Allocator.Persistent);
                rayResultCounts = new NativeArray<byte>(rayCount, Allocator.Persistent);

                var generateDirectionsJob = new FibonacciDirectionsJobParallel
                {
                    directions = rayDirections
                };

                generateDirectionsJob.Schedule(rayCount, 512).Complete();

                Debug.LogWarning("You changed the rayCount in the inspector. This will cause a crash in Builds, failsafe triggered: Recreated rayDirections array with new capacity.");
            }
            Debug.LogWarning("You changed the max bounces/rayCount in the inspector. This will cause a crash in Builds, failsafe triggered: Recreated rayResults array with new capacity.");
        }

        if (drawDebugArrays)
        {
            DEBUG_RayResults = rayResults.ToArray();
            DEBUG_RayResultCounts = rayResultCounts.ToArray();

            DEBUG_EchoRayDirections = echoRayDirections.ToArray();
            DEBUG_AudioTargetPositions = AudioTargetManager.AudioTargetPositions.JobBatchAsArray().ToArray();

            DEBUG_MaxMuffleHits = rayCount * MaxRayHits;
            DEBUG_MuffleRayHits = AudioTargetManager.MuffleRayHits.ToArray();

            DEBUG_MufflePercent01 = new float[AudioTargetManager.AudioTargetCount_JobBatch];
            for (int i = 0; i < AudioTargetManager.AudioTargetCount_JobBatch; i++)
            {
                DEBUG_MufflePercent01[i] = AudioTargetManager.AudioTargetRTData.JobBatch[i].AudioTargetSettings.muffle;
            }
        }
#endif

        AudioTargetManager.UpdateJobBatch();
        AudioColliderManager.UpdateJobBatch();

#if UNITY_EDITOR
        batchCycleMs = batchCycleStopwatch.ElapsedMilliseconds;
#endif


        #region Raycasting Job ParallelBatched

        // Create raytrace job and fire it
        audioRayTraceJob = new AudioRaytracerJobBatched
        {
            RayOrigin = (float3)transform.position + rayOrigin,
            RayDirections = rayDirections,

            AABBColliders = AudioColliderManager.AABBColliders.JobBatchAsArray(),
            AABBColliderCount = AudioColliderManager.AABBColliders.JobBatch.Length,

            OBBColliders = AudioColliderManager.OBBColliders.JobBatchAsArray(),
            OBBColliderCount = AudioColliderManager.OBBColliders.JobBatch.Length,

            SphereColliders = AudioColliderManager.SphereColliders.JobBatchAsArray(),
            SphereColliderCount = AudioColliderManager.SphereColliders.JobBatch.Length,

            AudioTargetPositions = AudioTargetManager.AudioTargetPositions.JobBatchAsArray(),

            MaxRayHits = MaxRayHits,
            MaxRayLife = maxRayLife,
            TotalAudioTargets = AudioTargetManager.AudioTargetCount_JobBatch,

            RayResults = rayResults,
            ResultCounts = rayResultCounts,

            EchoRayDirections = echoRayDirections,
            
            MuffleRayHits = AudioTargetManager.MuffleRayHits,
            PermeationStrengthRemains = AudioTargetManager.PermeationStrengthRemains,
        };

        int batchSize = (int)math.max(1, math.ceil((float)rayCount / AudioRaytracersManager.ToUseThreadCount));

        mainJobHandle = audioRayTraceJob.Schedule(rayCount, batchSize);

        #endregion


        #region Calculate Audio Target Data Job

        calculateAudioTargetDataJob = new ProcessAudioDataJob
        {
            RayResults = rayResults,
            RayResultCounts = rayResultCounts,

            EchoRayDirections = echoRayDirections,

            TotalAudioTargets = AudioTargetManager.AudioTargetCount_JobBatch,
            AudioTargetPositions = AudioTargetManager.AudioTargetPositions.JobBatchAsArray(),
            AudioTargetRTData = AudioTargetManager.AudioTargetRTData.JobBatchAsArray(),

            MuffleRayHits = AudioTargetManager.MuffleRayHits,

            MaxRayHits = MaxRayHits,
            RayCount = rayCount,
            RayOriginWorld = (float3)transform.position + rayOrigin,
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

    [Header("DEBUG Gizmos")]
    [SerializeField] private Color originColor = Color.green;
    [Space(10)]

    [SerializeField] private bool drawRayHitsGizmos = true;
    [SerializeField] private Color rayHitColor = Color.cyan;

    [SerializeField] private bool drawRayTrailsGizmos;
    [SerializeField] private Color rayTrailColor = new Color(0, 1, 0, 0.15f);

    [SerializeField] private bool drawReturnRayDirectionGizmos;
    [SerializeField] private Color rayReturnDirectionColor = new Color(0.5f, 0.25f, 0, 1f);

    [SerializeField] private bool drawReturnRayLastDirectionGizmos;
    [SerializeField] private Color rayReturnLastDirectionColor = new Color(1, 0.5f, 0, 1);

    [SerializeField] private bool drawReturnRaysAvgDirectionGizmos;
    [SerializeField] private Color rayReturnAvgDirectionColor = new Color(1, 0.5f, 0, 1);

    [Header("DEBUG Data Arrays")]
    [SerializeField] private bool drawDebugArrays = true;

    [SerializeField] private float3[] DEBUG_AudioTargetPositions;
    [SerializeField] private int DEBUG_MaxMuffleHits;
    [SerializeField] private ushort[] DEBUG_MuffleRayHits;
    [SerializeField] private float[] DEBUG_MufflePercent01;

    private AudioRayResult[] DEBUG_RayResults;
    private byte[] DEBUG_RayResultCounts;
    private half3[] DEBUG_EchoRayDirections;


    [SerializeField] private float raytracerMs;
    [SerializeField] private float batchCycleMs;
    private System.Diagnostics.Stopwatch raytracerJobsStopwatch;
    private System.Diagnostics.Stopwatch batchCycleStopwatch;


    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;

        float3 rayOrigin = (float3)transform.position + this.rayOrigin;


        if (DEBUG_RayResults != null && DEBUG_RayResults.Length != 0 && (drawRayHitsGizmos || drawRayTrailsGizmos || drawReturnRayDirectionGizmos || drawReturnRaysAvgDirectionGizmos))
        {
            AudioRayResult prevResult = new AudioRayResult();

            int maxRayHits = DEBUG_RayResults.Length / DEBUG_RayResultCounts.Length;
            int setResultAmountsCount = DEBUG_RayResultCounts.Length;
            int cSetResultCount;

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
                cSetResultCount = DEBUG_RayResultCounts[i];
                prevResult.DEBUG_HitPoint = (half3)rayOrigin;


                //ray hit markers and trails
                for (int i2 = 0; i2 < cSetResultCount; i2++)
                {
                    AudioRayResult result = DEBUG_RayResults[i * maxRayHits + i2];

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
