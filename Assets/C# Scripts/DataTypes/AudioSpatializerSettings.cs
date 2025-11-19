using UnityEngine;


[System.Serializable]
public struct AudioSpatializerSettings
{
    [Header("Panning Settings")]
    [Range(0f, 1f)]
    public float panStrength;

    [Range(0.25f, 10f)]
    public float overallGain;

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
    public MinMaxFloat highPassCutoff;

    [Space(5)]
    [Range(0f, 2f)]
    public float lowPassVolume;
    [Range(0f, 2f)]
    public float highPassVolume;

    [Header("Muffle Effect")]
    [Range(0f, 1f)]
    public float muffleStrength;
    public MinMaxFloat muffleCutoff;

    [Header("Reverb Effect")]
    [Range(0, 12)]
    public float maxReverbTime;
    [Range(0, 1)]
    public float reverbDecay;


    /// <summary>
    /// Default setttings for the audio spatializer.
    /// </summary>
    public static AudioSpatializerSettings Default => new AudioSpatializerSettings
    {
        panStrength = 0.8f,

        overallGain = 1,
        rearAttenuationStrength = 0.25f,

        distanceBasedPanning = true,
        maxPanDistance = 12,

        distanceBasedRearAttenuation = true,
        maxRearAttenuationDistance = 10f,

        maxElevationEffectDistance = 15f,

        lowPassCutoff = new MinMaxFloat(22000f, 5000f),
        highPassCutoff = new MinMaxFloat(20f, 500f),
        lowPassVolume = 0.85f,
        highPassVolume = 0.85f,

        muffleStrength = 0f,
        muffleCutoff = new MinMaxFloat(22000f, 0),

        maxReverbTime = 0.2f,
        reverbDecay = 0.1f,
    };
}
