using UnityEngine;


[System.Serializable]
public struct TransformOffset
{
    public Vector3 position;
    public Vector3 eulerRotation;
    public Vector3 scale;
    public Quaternion Rotation => Quaternion.Euler(eulerRotation);


    public TransformOffset(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        this.position = position;
        this.eulerRotation = rotation;
        this.scale = scale;
    }

    public static TransformOffset Default => new TransformOffset
    {
        position = Vector3.zero,
        eulerRotation = Vector3.zero,
        scale = Vector3.one,
    };
}