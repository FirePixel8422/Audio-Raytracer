#if UNITY_EDITOR
using Fire_Pixel.Utility;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;


public class DebugDataDisplay : UpdateMonoBehaviour
{
    [Tooltip("Average over this many seconds")]
    [SerializeField, EditorReadOnly]
    private float avgFPSCombineTime = 1f;

    [Space(10)]

    [SerializeField, EditorReadOnly] private string avgFps;
    [SerializeField, EditorReadOnly] private string avgFrameMs;
    [SerializeField, EditorReadOnly] private int drawCalls;
    [SerializeField, EditorReadOnly] private int setPassCalls;
    [SerializeField, EditorReadOnly] private int tris;

    [Header("Global Memory (MB)")]
    [SerializeField, EditorReadOnly] private string totalAllocatedMemoryMB;
    [SerializeField, EditorReadOnly] private string totalReservedMemoryMB;
    [SerializeField, EditorReadOnly] private string totalUnusedReservedMemoryMB;
    [SerializeField, EditorReadOnly] private string monoHeapSizeMB;
    [SerializeField, EditorReadOnly] private string monoUsedSizeMB;

    [Space(10)]

    [Header("Component Counts: (Active/Total)")]
    [SerializeField, EditorReadOnly] private string gameObjectCount;
    [SerializeField, EditorReadOnly] private string componentCount;
    [SerializeField, EditorReadOnly] private string uiComponentCount;
    [SerializeField, EditorReadOnly] private int activeAudioSources;

    private static readonly CultureInfo enCulture = new CultureInfo("en-US");

    private struct FrameData
    {
        public float Timestamp;
        public float DeltaTime;
    }

    private readonly Queue<FrameData> frameTimes = new Queue<FrameData>();


    private void Awake() => ReloadExpensiveStats();

    protected override void OnUpdate()
    {
        float currentTime = Time.time;
        float deltaTime = Time.deltaTime;

        // Track frame times for rolling avg
        frameTimes.Enqueue(new FrameData { Timestamp = currentTime, DeltaTime = deltaTime });
        while (frameTimes.Count > 0 && currentTime - frameTimes.Peek().Timestamp > avgFPSCombineTime)
            frameTimes.Dequeue();

        // Calculate averages
        float totalDeltaTime = 0f;
        foreach (var frame in frameTimes)
            totalDeltaTime += frame.DeltaTime;

        int count = frameTimes.Count;
        if (count > 0)
        {
            float avgDeltaTime = totalDeltaTime / count;
            avgFrameMs = (avgDeltaTime * 1000f).ToString("F2", enCulture) + " ms";
            avgFps = (1f / avgDeltaTime).ToString("F1", enCulture) + " fps";
        }
        else
        {
            avgFrameMs = "0 ms";
            avgFps = "0 FPS";
        }

        // Cheap per-frame stats
        drawCalls = UnityEditor.UnityStats.drawCalls;
        setPassCalls = UnityEditor.UnityStats.setPassCalls;
        tris = UnityEditor.UnityStats.triangles;

        totalAllocatedMemoryMB = (Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)).ToString("N0", enCulture) + " mb";
        totalReservedMemoryMB = (Profiler.GetTotalReservedMemoryLong() / (1024 * 1024)).ToString("N0", enCulture) + " mb";
        totalUnusedReservedMemoryMB = (Profiler.GetTotalUnusedReservedMemoryLong() / (1024 * 1024)).ToString("N0", enCulture) + " mb";
        monoHeapSizeMB = (Profiler.GetMonoHeapSizeLong() / (1024 * 1024)).ToString("N0", enCulture) + " mb";
        monoUsedSizeMB = (Profiler.GetMonoUsedSizeLong() / (1024 * 1024)).ToString("N0", enCulture) + " mb";
    }

    [InspectorButton("ReloadExpensiveStatistics")]
    public void ReloadExpensiveStats()
    {
        int totalGameObjectCount = this.FindObjectsOfType<GameObject>(true).Length;
        int activeGameObjectCount = this.FindObjectsOfType<GameObject>(true).Count(c => c.activeInHierarchy);
        gameObjectCount = $"{activeGameObjectCount}/{totalGameObjectCount}";

        int totalComponentCount = this.FindObjectsOfType<Component>(true).Count(c => c is not Transform && c.GetComponent<RectTransform>() == null);
        int activeComponentCount = this.FindObjectsOfType<Component>(true).Count(c => c is not Transform && c.GetComponent<RectTransform>() == null && c.gameObject.activeInHierarchy && (c is not Behaviour b || b.enabled));
        componentCount = $"{activeComponentCount}/{totalComponentCount}";

        int totalUIComponentCount = this.FindObjectsOfType<Component>(true).Count(c => c.GetComponent<RectTransform>() != null);
        int activeUIComponentCount = this.FindObjectsOfType<Component>(true).Count(c => c.GetComponent<RectTransform>() != null && c.gameObject.activeInHierarchy && (c is not Behaviour b || b.enabled));
        uiComponentCount = $"{activeUIComponentCount}/{totalUIComponentCount}";

        activeAudioSources = this.FindObjectsOfType<AudioSource>(true).Count(c => c.isPlaying);
    }
}
#endif
