using UnityEngine;



[CreateAssetMenu(fileName = "Default Spatializer Settings", menuName = "Audio Spatializer/Settings")]
public class AudioSpatializerSettingsSO : ScriptableObject
{
    public AudioSpatializerSettings settings = AudioSpatializerSettings.Default;
}