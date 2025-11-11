using UnityEngine;


public abstract class AudioCollider : MonoBehaviour
{
    [Header("Set this to true if collider never moves at runtime")]
    public bool IsStatic = true;

    public short AudioColliderId;
    public short AudioTargetId;

    protected Vector3 lastWorldPosition;
    protected Vector3 lastGlobalScale;


    private void Awake()
    {
        if (TryGetComponent(out AudioTargetRT audioTargetRT))
        {
            AudioTargetId = audioTargetRT.Id;
        }
        else
        {
            AudioTargetId = -1;
        }

        if (IsStatic == false)
        {
            UpdateSavedData();
            AudioColliderManager.OnColliderUpdate += CheckColliderTransformation;
        }
    }
    protected virtual void UpdateSavedData()
    {
        lastWorldPosition = transform.position;
        lastGlobalScale = transform.lossyScale;
    }


    public virtual ColliderType GetColliderType()
    {
        return ColliderType.None;
    }

    /// <summary>
    /// Add audio collider as struct data into the corresponding native array at correct index and increment index
    /// </summary>
    public virtual void AddToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs) { }

    /// <summary>
    /// Update audio collider as struct data into the corresponding native array at correct index based on assigned AudioColliderId
    /// </summary>
    public virtual void UpdateToAudioSystem(NativeListBatch<ColliderAABBStruct> aabbStructs, NativeListBatch<ColliderOBBStruct> obbStructs, NativeListBatch<ColliderSphereStruct> sphereStructs) { }

    private void OnEnable() => AudioColliderManager.AddColiderToSystem(this);
    //private void OnDisable() => AudioColliderManager.RemoveColiderFromSystem(this);
    private void OnDestroy()
    {
        if (IsStatic == false)
        {
            AudioColliderManager.OnColliderUpdate -= CheckColliderTransformation;
        }

        AudioColliderManager.RemoveColiderFromSystem(this);
    }

    protected virtual void CheckColliderTransformation() { }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = AudioColliderManager.ColliderGizmosSelectedColor;
        DrawColliderGizmo();
    }
    public virtual void DrawColliderGizmo() { }
#endif
}