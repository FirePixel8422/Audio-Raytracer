using System;
using UnityEngine;


[Serializable]
public struct UpdateValue<T>
{
    [SerializeField] private T value;
    public Action<T> OnValueChanged;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }
    }
}