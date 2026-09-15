using CrowSupport.Events;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioTargetRTEvent", menuName = "ScriptableObjects/Events/AudioTargetRTEvent", order = -1005)]
public class AudioTargetRTEventSO : GameEventSO<(AudioTargetRT Target, AudioTargetChangeType ChangeType)>
{
}

public enum AudioTargetChangeType
{
    Add,
    Update,
    Remove
}