using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct MinMaxFloat
{
    [SerializeField] private float2 minMax;

    public float Min => minMax.x;
    public float Max => minMax.y;

    public MinMaxFloat(float min, float max)
    {
        minMax = new float2(min, max);
    }
}