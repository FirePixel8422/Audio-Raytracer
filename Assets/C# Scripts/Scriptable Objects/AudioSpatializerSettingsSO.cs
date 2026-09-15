using UnityEngine;


[CreateAssetMenu(fileName = "Default Spatializer Settings", menuName = "ScriptableObjects/Audio/Spatializer Settings", order = -1000)]
public class AudioSpatializerSettingsSO : ScriptableObject
{
    public AudioSpatializerSettings settings = AudioSpatializerSettings.Default;
}