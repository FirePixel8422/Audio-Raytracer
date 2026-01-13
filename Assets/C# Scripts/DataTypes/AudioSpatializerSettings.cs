using UnityEngine;


[System.Serializable]
public struct AudioSpatializerSettings
{
    [Header("Panning Settings")]
    [Range(0f, 1f)]
    public float panStrength;

    [Header("Rear Attenuation")]
    [Range(0f, 1f)]
    public float rearAttenuationStrength;

    [Header("Distance Based Panning")]
    public bool distanceBasedPanning;
    public float maxPanDistance;

    [Header("Rear Attenuation Distance")]
    public bool distanceBasedRearAttenuation;
    public float maxRearAttenuationDistance;

    [Header("Elevation Influence Falloff And Freq Effect")]
    public float maxElevationEffectDistance;

    [Space(5)]

    public MinMaxFloat lowPassCutoff;
    [Range(0f, 2f)]
    public float lowPassVolume;

    [Space(2.5f)]

    public MinMaxFloat highPassCutoff;
    [Range(0f, 2f)]
    public float highPassVolume;

    [Header("Muffle Effect")]
    public NativeSampledAnimationCurve MuffleCurve;
    public MinMaxFloat muffleCutoff;

    [Header("Reverb Effect")]
    [Range(0, 1)]
    public float reverbDecayFactor;
    [Range(0, 1)]
    public float reverbAllpassGain;
    [Range(0, 2)]
    public float wetBoostMultiplier;


    /// <summary>
    /// Default setttings for the audio spatializer.
    /// </summary>
    public static AudioSpatializerSettings Default => new AudioSpatializerSettings
    {
        panStrength = 0.8f,

        rearAttenuationStrength = 0.2f,

        distanceBasedPanning = true,
        maxPanDistance = 5,

        distanceBasedRearAttenuation = true,
        maxRearAttenuationDistance = 15,

        maxElevationEffectDistance = 12,

        lowPassCutoff = new MinMaxFloat(5000, 22000),
        highPassCutoff = new MinMaxFloat(25, 150),
        lowPassVolume = 0.85f,
        highPassVolume = 1.15f,

        MuffleCurve = NativeSampledAnimationCurve.Default,
        muffleCutoff = new MinMaxFloat(75, 8000),

        reverbDecayFactor = 0.805f,
        reverbAllpassGain = 0.7f,
        wetBoostMultiplier = 0,
    };
}
