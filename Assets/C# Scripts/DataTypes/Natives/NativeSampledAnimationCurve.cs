using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    /// <summary>
    /// Wrapper for AnimationCurve that allows sampling at a fixed number of points, making Evaluate() much cheaper.
    /// </summary>
    [InspectorButtonCompatibility]
    [System.Serializable, BurstCompile]
    public class NativeSampledAnimationCurve
    {
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(1, 1, 0, 0);

        [Tooltip("More samples = more accurate, but more memory usage")]
        [Range(2, 500)]
        [SerializeField] private int _sampleCount = 50;

        private NativeArray<float> _bakedCurve;
        public bool IsCreated => _bakedCurve.IsCreated;

        private float _minTime;
        private float _maxTime;


        /// <summary>
        /// Bake the AnimationCurve into the fixed number of samples.
        /// </summary>
        [InspectorButton("(Re-)Bake Curve", false)]
        public void Bake()
        {
#if ENABLE_DEBUG_SYSTEMS
            if (_curve.keys.Length == 0)
            {
                DebugLogger.LogError("AnimationCurve is null!");
                return;
            }
#endif

            _sampleCount = math.max(_sampleCount, 2);

            _bakedCurve.DisposeIfCreated();
            _bakedCurve = new NativeArray<float>(_sampleCount, Allocator.Persistent);

            _minTime = _curve.keys[0].time;
            _maxTime = _curve.keys[^1].time;

            float timeRange = _maxTime - _minTime;

            for (int i = 0; i < _sampleCount; i++)
            {
                float percent = (float)i / (_sampleCount - 1);
                float time = _minTime + percent * timeRange;

                _bakedCurve[i] = _curve.Evaluate(time);
            }
        }
        public void DisposeIfCreated()
        {
            _bakedCurve.DisposeIfCreated();
        }

        /// <summary>
        /// Get value from the baked curve based on <paramref name="time"/>
        /// </summary>
        public float Evaluate(float time)
        {
#if UNITY_EDITOR
            if (_bakedCurve == null || _bakedCurve.Length != _sampleCount)
            {
                DebugLogger.LogError("NativeSampledAnimationCurve: Baked curve is not initialized properly, call Bake before use. This will throw in builds.");
                Bake();
            }
#endif
            float percent01 = math.saturate((time - _minTime) / (_maxTime - _minTime));
            return EvaluateWithBurst(_bakedCurve, _sampleCount, percent01);
        }
        /// <summary>
        /// Get value from the baked curve based on <paramref name="percent01"/> (Slightly faster than .Evaluate(time))
        /// </summary>
        public float Evaluate01(float percent01)
        {
#if UNITY_EDITOR
            if (_bakedCurve == null || _bakedCurve.Length != _sampleCount)
            {
                DebugLogger.LogError("NativeSampledAnimationCurve: Baked curve is not initialized properly, call Bake before use. This will throw in builds.");
                Bake();
            }
#endif

            return EvaluateWithBurst(_bakedCurve, _sampleCount, percent01);
        }

        /// <summary>
        /// Get value based on percent (0-1) from the baked curve, compiled with burst for super fast evaluation.
        /// </summary>
        [BurstCompile]
        private static float EvaluateWithBurst(in NativeArray<float> bakedCurve, int sampleCount, float percent01)
        {
            float curvePosition = percent01 * (sampleCount - 1);

            int floorIndex = (int)math.floor(curvePosition);
            int ceilIndex = (int)curvePosition;

            return math.lerp(bakedCurve[floorIndex], bakedCurve[ceilIndex], curvePosition - floorIndex);
        }
    }
}