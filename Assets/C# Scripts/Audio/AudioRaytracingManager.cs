using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class AudioRaytracingManager : MonoBehaviour
{
    private static AudioRaytracingManager Instance;
    

    [Header("Run Raytracing async on background threads in parallel")]
    [Space(-5)]
    [Header("WARNING: If false will block the main thread every frame until finished")]
    [SerializeField] private bool computeAsync = true;
    public static bool ComputeAsync => Instance.computeAsync;

    [Header("Max threads to use for raytracing")]
    [SerializeField] private int maxThreadCount = 3;
    public static int ToUseThreadCount => math.min(Instance.maxThreadCount, JobsUtility.JobWorkerCount);


    [SerializeField] private AudioColliderManager colliderManager;
    public static AudioColliderManager ColliderManager => Instance.colliderManager;

    [SerializeField] private AudioTargetManager audioTargetManager;
    public static AudioTargetManager AudioTargetManager => Instance.audioTargetManager;



    private void Awake()
    {
        Instance = this;
        colliderManager.Init();
        audioTargetManager.Init();
    }
    private void OnValidate()
    {
        Instance = this;
    }
    private void OnDrawGizmosSelected()
    {
        colliderManager.DrawGizmos();
    }
}
