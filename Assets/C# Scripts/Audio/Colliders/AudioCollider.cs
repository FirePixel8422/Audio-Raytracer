using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public abstract class AudioCollider : MonoBehaviour
{
    [Header("Set this to true if collider never changes at runtime")]
    public bool IsStatic;

    private bool initialized;
    public int AudioColliderId;


    private void Start()
    {
        if (initialized) return;

        AudioColliderManager.Instance.AddColiderToSystem(this);
    }

    /// <summary>
    /// Add audio collider as struct data into the corresponding native array at correct index and increment index
    /// </summary>
    public virtual void AddToAudioSystem(
        ref NativeArray<ColliderAABBStruct> aabbStructs, ref int cAABBId,
        ref NativeArray<ColliderOBBStruct> obbStructs, ref int cOBBId,
        ref NativeArray<ColliderSphereStruct> sphereStructs, ref int cSphereId,
        int audioColliderId)
    {
        initialized = true;
        AudioColliderId = audioColliderId;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected() => DrawColliderGizmo();
    public virtual void DrawColliderGizmo() { }
#endif
}