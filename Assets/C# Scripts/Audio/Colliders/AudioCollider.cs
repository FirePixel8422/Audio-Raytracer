using UnityEngine;


public abstract class AudioCollider : MonoBehaviour
{
    [Header("Set this to true if collider never moves at runtime")]
    public bool IsStatic = true;
    [Header("Collider doesnt UPDATE with scale if true (improves performance)")]
    public bool IgnoreScale = true;

    public short AudioColliderId;
    public short AudioTargetId;

    protected Transform cachedTransform;
    protected Vector3 lastWorldPosition;
    protected Vector3 lastGlobalScale;


    private void Awake()
    {
        cachedTransform = transform;

        bool hasAudioTargetRT = TryGetComponent(out AudioTargetRT audioTargetRT);

        AudioTargetId = hasAudioTargetRT ? audioTargetRT.Id : (short)-1;

        if (IsStatic == false)
        {
            UpdateSavedData(cachedTransform.position, IgnoreScale ? Vector3.zero : cachedTransform.lossyScale);
            AudioColliderManager.OnColliderUpdate += CheckColliderTransformation;
        }
    }
    protected virtual void UpdateSavedData(Vector3 cWorldPosition, Vector3 cGlobalScale)
    {
        lastWorldPosition = cWorldPosition;

        if (IgnoreScale) return;
        lastGlobalScale = cGlobalScale;
    }

    public virtual ColliderType GetColliderType()
    {
        return ColliderType.None;
    }

    /// <summary>
    /// Add audio collider as struct data into the corresponding native array at correct index and increment index
    /// </summary>
    public virtual void AddToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs) { }

    /// <summary>
    /// Update audio collider as struct data into the corresponding native array at correct index based on assigned AudioColliderId
    /// </summary>
    public virtual void UpdateToAudioSystem(NativeJobBatch<ColliderAABBStruct> aabbStructs, NativeJobBatch<ColliderOBBStruct> obbStructs, NativeJobBatch<ColliderSphereStruct> sphereStructs) { }


    private void OnEnable() => AudioColliderManager.AddColiderToSystem(this);
    private void OnDisable() => AudioColliderManager.RemoveColiderFromSystem(this);

    private void OnDestroy()
    {
        if (IsStatic == false)
        {
            AudioColliderManager.OnColliderUpdate -= CheckColliderTransformation;
        }
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