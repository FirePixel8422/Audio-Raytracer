using UnityEngine;

[System.Serializable]
public struct AudioSpatializerSettings
{
    [Header("Panning Settings")]
    [Range(0f, 1f)]
    public float PanStrength;

    [Header("Rear Attenuation")]
    [Range(0f, 1f)]
    public float RearAttenuationStrength;

    [Header("Distance Based Panning")]
    public bool DistanceBasedPanning;
    public float MaxPanDistance;

    [Header("Rear Attenuation Distance")]
    public bool DistanceBasedRearAttenuation;
    public float MaxRearAttenuationDistance;

    [Header("Elevation Influence Falloff And Freq Effect")]
    public float MaxElevationEffectDistance;

    [Space(5)]
    public MinMaxFloat LowPassCutoff;

    [Range(0f, 2f)]
    public float LowPassVolume;

    [Space(2.5f)]
    public MinMaxFloat HighPassCutoff;

    [Range(0f, 2f)]
    public float HighPassVolume;

    [Header(">>Muffle Effect<<")]
    public NativeSampledAnimationCurve MuffleCurve;
    public MinMaxFloat MuffleCutoff;

    [Header(">>Reverb Strength and Volume settings<<")]
    public MinMaxFloat ReverbDryLevel;
    public NativeSampledAnimationCurve ReverbStrengthCurve;
    public MinMaxFloat ReverbDryBoost;
    public NativeSampledAnimationCurve ReverbVolumeCurve;

    public static AudioSpatializerSettings Default => new AudioSpatializerSettings
    {
        PanStrength = 0.8f,

        RearAttenuationStrength = 0.2f,

        DistanceBasedPanning = true,
        MaxPanDistance = 5,

        DistanceBasedRearAttenuation = true,
        MaxRearAttenuationDistance = 15,

        MaxElevationEffectDistance = 12,

        LowPassCutoff = new MinMaxFloat(5000, 22000),
        HighPassCutoff = new MinMaxFloat(25, 150),
        LowPassVolume = 0.85f,
        HighPassVolume = 1.15f,

        MuffleCurve = NativeSampledAnimationCurve.Default,
        MuffleCutoff = new MinMaxFloat(75, 8000),

        ReverbDryLevel = new MinMaxFloat(0, -2000),
        ReverbStrengthCurve = NativeSampledAnimationCurve.Default,
        ReverbDryBoost = new MinMaxFloat(1, 3),
        ReverbVolumeCurve = NativeSampledAnimationCurve.Default,
    };

    public void Bake()
    {
        MuffleCurve.Bake();
        ReverbStrengthCurve.Bake();
        ReverbVolumeCurve.Bake();
    }

    public void Dispose()
    {
        MuffleCurve.Dispose();
        ReverbStrengthCurve.Dispose();
        ReverbVolumeCurve.Dispose();
    }
}
