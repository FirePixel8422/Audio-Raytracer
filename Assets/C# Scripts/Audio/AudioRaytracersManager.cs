using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class AudioRaytracersManager : MonoBehaviour
{
    public static AudioRaytracersManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    

    [Header("Run Raytracing async on background threads in parallel")]
    [Header("WARNING: If false will block the main thread every frame until all rays are calculated")]
    [SerializeField] private bool computeAsync = true;
    public static bool ComputeAsync => Instance.computeAsync;

    [Header("Max threads to use for raytracing (will use less if not enough threads available)")]
    [SerializeField] private int maxThreadCount = 3;
    public static int ToUseThreadCount => math.min(Instance.maxThreadCount, JobsUtility.JobWorkerCount);
}
