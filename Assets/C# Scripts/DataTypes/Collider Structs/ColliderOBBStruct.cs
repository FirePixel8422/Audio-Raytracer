using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;


[System.Serializable]
[BurstCompile]
public struct ColliderOBBStruct
{
    public half3 center;
    public half3 size;

    private halfQuaternion rotation;
    public quaternion Rotation
    {
        get => rotation.QuaternionValue;
        set
        {
            rotation.QuaternionValue = value;
        }
    }

    [Header("How thick is this wall for permeation calculations")]
    public half thicknessMultiplier;

    [HideInInspector]
    public short audioTargetId;


    public static ColliderOBBStruct Default => new ColliderOBBStruct()
    {
        size = new half3(0.5f),
        rotation = new halfQuaternion(quaternion.identity),
        thicknessMultiplier = (half)1,
        audioTargetId = -1,
    };
}