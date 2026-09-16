using Fire_Pixel.Utility;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


[RequireComponent(typeof(AudioTargetManager), typeof(AudioColliderManager))]
public class AudioRaytracingManager : MonoBehaviour
{
    public static AudioRaytracingManager Instance { get; private set; }


    [field: Header("Run Raytracing async on background threads in parallel")]
    [field: Tooltip("WARNING: If false will block the main thread until finished")]
    [field: SerializeField] public bool ComputeAsync { get; private set; } = true;

    [Tooltip("Max threads to use for raytrace jobs")]
    [SerializeField] private int MaxThreadCount = 3;
    public int ToUseThreadCount => math.min(MaxThreadCount, JobsUtility.JobWorkerCount);

    public AudioTargetManager AudioTargetManager { get; private set; }
    public AudioColliderManager ColliderManager { get; private set; }




    private void Awake()
    {
        Instance = this;

        AudioTargetManager = GetComponent<AudioTargetManager>();
        ColliderManager = GetComponent<AudioColliderManager>();

        ColliderManager.Init();
        AudioTargetManager.Init();
    }
    private void OnDestroy()
    {
        CallbackScheduler.RegisterCallback(() =>
        {
            ColliderManager.Dispose();
            AudioTargetManager.Dispose();
        }, CallbackType.LateApplicationQuit);
    }



#if UNITY_EDITOR
    public static AudioRaytracingManager EditorInstance { get; private set; }
    private void OnValidate()
    {
        EditorInstance = this;

        AudioTargetManager = GetComponent<AudioTargetManager>();
        ColliderManager = GetComponent<AudioColliderManager>();
    }
#endif
}
