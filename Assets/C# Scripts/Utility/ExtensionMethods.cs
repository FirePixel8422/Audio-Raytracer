using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public static class ExtensionMethods
{
    #region Invoke

    /// <summary>
    /// Call function after a delay
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="f">Function to call.</param>
    /// <param name="delay">Wait time before calling function.</param>
    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    public static void Invoke<T>(this MonoBehaviour mb, Action<T> f, T param, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, param, delay));
    }

    private static IEnumerator InvokeRoutine(Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke();
    }

    private static IEnumerator InvokeRoutine<T>(Action<T> f, T param, float delay)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke(param);
    }

    #endregion


    #region Transform Logic

    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
    }
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot, bool keepLocalScale)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
        if (!keepLocalScale)
        {
            trans.localScale = Vector3.one;
        }
    }

    #endregion


    #region (Try)GetComponent(s)

    public static bool TryGetComponents<T>(this Transform trans, out T[] components) where T : UnityEngine.Object
    {
        components = trans.GetComponents<T>();

        return components.Length > 0;
    }

    public static bool TryGetComponentInChildren<T>(this Transform trans, out T component, bool includeInactive = false) where T : UnityEngine.Object
    {
        component = trans.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
    public static bool TryGetComponentsInChildren<T>(this Transform trans, out T[] components, bool includeInactive) where T : UnityEngine.Object
    {
        components = trans.GetComponentsInChildren<T>(includeInactive);

        return components.Length > 0;
    }

    public static bool TryGetComponentInParent<T>(this Transform trans, out T component) where T : UnityEngine.Object
    {
        component = trans.GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentsInParent<T>(this Transform trans, out T[] component) where T : UnityEngine.Object
    {
        component = trans.GetComponentsInParent<T>();
        return component != null;
    }

    public static bool TryFindObjectOfType<T>(this UnityEngine.Object obj, out T component, bool includeInactive = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

        component = UnityEngine.Object.FindFirstObjectByType<T>(findObjectsInactive);
        return component != null;
    }
    public static bool TryFindObjectsOfType<T>(this UnityEngine.Object obj, out T[] component, bool includeInactive = false, bool sortByInstanceID = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        component = UnityEngine.Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
        return component != null;
    }

    // Unity 6s new FindobjectOfType is stupid
    public static T FindObjectOfType<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindFirstObjectByType<T>();
    }

    public static T[] FindObjectsOfType<T>(this UnityEngine.Object obj, bool includeInactive = true, bool sortByInstanceID = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        return UnityEngine.Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
    }

    #endregion


    #region HasComponent

    public static bool HasComponent<T>(this Transform trans) where T : Component
    {
        return trans.GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInChildren<T>(includeInactive) != null;
    }

    public static bool HasComponentInParent<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInParent<T>(includeInactive) != null;
    }

    #endregion


    #region DisposeIfCreated for Native Collections

    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeArray<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeList<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeReference<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeHashSet<T> array) where T : unmanaged, IEquatable<T>
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<Tkey, TValue>(this NativeHashMap<Tkey, TValue> array) where Tkey : unmanaged, IEquatable<Tkey> where TValue : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }

    #endregion


    #region PlayClip with Pitch and Clip overloads for AudioSource

    /// <summary>
    /// Lets AudioSource play selected clip with selected pitch
    /// </summary>
    public static void PlayClipWithPitch(this AudioSource source, AudioClip clip, float pitch)
    {
        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
    /// <summary>
    /// Lets AudioSource play with selected pitch
    /// </summary>
    public static void PlayWithPitch(this AudioSource source, float pitch)
    {
        source.pitch = pitch;
        source.Play();
    }
    /// <summary>
    /// Lets AudioSource play with selected pitch
    /// </summary>
    public static void PlayOneShotWithPitch(this AudioSource source, float pitch)
    {
        source.pitch = pitch;
        source.PlayOneShot(source.clip);
    }
    /// <summary>
    /// Lets AudioSource play selected clip with selected pitch
    /// </summary>
    public static void PlayOneShotClipWithPitch(this AudioSource source, AudioClip clip, float pitch)
    {
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }

    #endregion


    #region SetActiveSmart for GameObjects and Components

    /// <summary>
    /// Set the active state of a Behaviour component only if the state is different from the current state.
    /// </summary>
    public static void SetActiveStateSmart(this Behaviour comp, bool state)
    {
        if (comp.enabled != state)
        {
            comp.enabled = state;
        }
    }

    /// <summary>
    /// Set the active state of a GameObject only if the state is different from the current state.
    /// </summary>
    public static void SetActiveSmart(this GameObject obj, bool state)
    {
        if (obj.activeInHierarchy != state)
        {
            obj.SetActive(state);
        }
    }

    #endregion



    ///// <returns>SurfaceType enum of the target collider, returns SurfaceType.None if there is no <see cref="SurfaceTypeIdentifier"/> attached to targetr collider</returns>
    //public static SurfaceType GetSurfaceType(this Collider collider)
    //{
    //    if (collider.TryGetComponent(out SurfaceTypeIdentifier identifier))
    //    {
    //        return identifier.SurfaceType;
    //    }
    //    return SurfaceType.Metal;
    //}

    ///// <summary>
    ///// Get PlayerGameId through ClientManager using OwnerClientId.
    ///// </summary>
    //public static int GetOwnerClientGameId(this NetworkObject networkObj)
    //{
    //    return ClientManager.GetClientGameId(networkObj.OwnerClientId);
    //}

    /// <summary>
    /// Try finding an action by name, returns true if found, false if not. Outputs the found action
    /// </summary>
    public static bool TryFindAction(this InputActionAsset actionAsset, string actionName, out InputAction action)
    {
        action = actionAsset.FindAction(actionName);
        return action != null;
    }

    /// <summary>
    /// Assign value <typeparamref name="T"/> to ref of captured variable (use this to write back to arrays)
    /// </summary>
    public static void AssignValueToRef<T>(this ref T target, T value) where T : unmanaged
    {
        target = value;
    }

    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static void Shuffle<T>(this T[] targetArray)
    {
        int n = targetArray.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (targetArray[i], targetArray[j]) = (targetArray[j], targetArray[i]);
        }
    }

    /// <summary>
    /// Replaces the element at <paramref name="targetId"/> with the element at <paramref name="backId"/>, 
    /// then clears the element at <paramref name="backId"/> by assigning <paramref name="nullValue"/>.
    /// Useful for implementing a swap-back removal in fixed-size arrays.
    /// </summary>
    public static void RemoveAtSwapBack<T>(this T[] targetArray, int targetId, int backId, T nullValue = default)
    {
        targetArray[targetId] = targetArray[backId];
        targetArray[backId] = nullValue;
    }
}