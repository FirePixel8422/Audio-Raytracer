using Fire_Pixel.Utility;
using UnityEngine;


[CreateAssetMenu(fileName = "Default Material Properties", menuName = "ScriptableObjects/Audio/Material Properties", order = -1000)]
public class AudioMaterialPropertiesSO : ScriptableObject
{
    public AudioMaterialProperties MaterialProperties = AudioMaterialProperties.Default;

    [EditorReadOnly] public short Id;
}