using Unity.Collections;
using UnityEngine;


public abstract class AudioCollider : MonoBehaviour
{
    [Header("Set this to true if collider never changes at runtime")]
    public bool IsStatic;

    private bool initialized;
    public short AudioColliderId;


    private void Start()
    {
        if (initialized) return;

        AudioColliderManager.AddColiderToSystem(this);
    }


    public virtual ColliderType GetColliderType()
    {
        return ColliderType.None;
    }

    /// <summary>
    /// Add audio collider as struct data into the corresponding native array at correct index and increment index
    /// </summary>
    public virtual void AddToAudioSystem(ref NativeList<ColliderAABBStruct> aabbStructs, ref NativeList<ColliderOBBStruct> obbStructs, ref NativeList<ColliderSphereStruct> sphereStructs)
    {
        initialized = true;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = AudioColliderManager.ColliderGizmosSelectedColor;
        DrawColliderGizmo();
    }
    public virtual void DrawColliderGizmo() { }
#endif
}