using UnityEngine;
using Fire_Pixel.Utility;


public class UpdateMonoBehaviour : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        CallbackScheduler.RegisterCallback(OnUpdate, CallbackType.Update);
    }
    protected virtual void OnDisable()
    {
        CallbackScheduler.UnRegisterCallback(OnUpdate, CallbackType.Update);
    }
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void OnUpdate() { }
}