using UnityEngine;
using Fire_Pixel.Utility;

public class UpdateMonoBehaviour : MonoBehaviour
{
    protected virtual void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    protected virtual void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    protected virtual void OnUpdate()
    {

    }
}