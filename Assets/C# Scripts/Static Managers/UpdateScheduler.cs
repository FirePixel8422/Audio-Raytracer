using System;
using UnityEngine;


/// <summary>
/// Uitlity class to have an optimized easy acces to Updte Callbacks by using an Action based callback system
/// </summary>
public static class UpdateScheduler
{
#pragma warning disable UDR000
    private static Action OnUpdate;
    private static Action OnLateUpdate;
    private static Action OnFixedUpdate;

    private static Action OnLateDestroy;
    private static Action OnLateApplicationQuit;
#pragma warning restore UDR0002


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        UpdateCallbackManager gameManager = new GameObject("UpdateCallbackManager").AddComponent<UpdateCallbackManager>();
        gameManager.gameObject.isStatic = true;

        GameObject.DontDestroyOnLoad(gameManager.gameObject);
    }


    #region void Update

    /// <summary>
    /// Register a method to call every frame like Update()
    /// </summary>
    public static void RegisterUpdate(Action action)
    {
#pragma warning disable UDR0004
        OnUpdate += action;
#pragma warning restore UDR0004
    }
    /// <summary>
    /// Unregister a registerd method for Update()
    /// </summary>
    public static void UnRegisterUpdate(Action action)
    {
        OnUpdate -= action;
    }
    /// <summary>
    /// Register or Unregister a method for Update() based on bool <paramref name="register"/>
    /// </summary>
    public static void ManageUpdate(Action action, bool register)
    {
        if (register)
        {
            RegisterUpdate(action);
        }
        else
        {
            UnRegisterUpdate(action);
        }
    }

    #endregion


    #region void LateUpdate

    /// <summary>
    /// Register a method to call every frame like Update()
    /// </summary>
    public static void RegisterLateUpdate(Action action)
    {
#pragma warning disable UDR0004
        OnLateUpdate += action;
#pragma warning restore UDR0004
    }
    /// <summary>
    /// Unregister a registerd method for Update()
    /// </summary>
    public static void UnRegisterLateUpdate(Action action)
    {
        OnLateUpdate -= action;
    }
    /// <summary>
    /// Register or Unregister a method for Update() based on bool <paramref name="register"/>
    /// </summary>
    public static void ManageLateUpdate(Action action, bool register)
    {
        if (register)
        {
            RegisterLateUpdate(action);
        }
        else
        {
            UnRegisterLateUpdate(action);
        }
    }

    #endregion


    #region void FixedUpdate

    /// <summary>
    /// Register a method to call every frame like FixedUpdate()
    /// </summary>
    public static void RegisterFixedUpdate(Action action)
    {
#pragma warning disable UDR0004
        OnFixedUpdate += action;
#pragma warning restore UDR0004
    }
    /// <summary>
    /// Unregister a registerd method for FixedUpdate()
    /// </summary>
    public static void UnRegisterFixedUpdate(Action action)
    {
        OnFixedUpdate -= action;
    }
    /// <summary>
    /// Register or Unregister a method for FixedUpdate() based on bool <paramref name="register"/>
    /// </summary>
    public static void ManageFixedUpdate(Action action, bool register)
    {
        if (register)
        {
            RegisterFixedUpdate(action);
        }
        else
        {
            UnRegisterFixedUpdate(action);
        }
    }

    #endregion


    public static void CreateLateOnDestroyCallback(Action action)
    {
        OnLateDestroy += action;
    }

    public static void CreateLateOnApplicationQuitCallback(Action action)
    {
        OnLateApplicationQuit += action;
    }


    /// <summary>
    /// Handle Update Callbacks and batch them for every script by an event based register system
    /// </summary>
    private class UpdateCallbackManager : MonoBehaviour
    {
        private void Update()
        {
            OnUpdate?.Invoke();
        }
        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();

            if (OnLateDestroy != null)
            {
                OnLateDestroy.Invoke();
                OnLateDestroy = null;
            }
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void OnApplicationQuit()
        {
            OnLateUpdate += () =>
            {
                if (OnLateApplicationQuit != null)
                {
                    OnLateApplicationQuit.Invoke();
                    OnLateApplicationQuit = null;
                }
            };
        }
        private void OnDestroy()
        {
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
        }
    }
}