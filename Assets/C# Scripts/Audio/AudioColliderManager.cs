using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;



public class AudioColliderManager : MonoBehaviour
{
    public static AudioColliderManager Instance { get; private set; }


    private List<AudioCollider> colliders;


    public NativeArray<ColliderAABBStruct> AABBColliders;
    private int aabbCount;
    public int AABBCount => aabbCount;


    public NativeArray<ColliderOBBStruct> OBBColliders;
    private int obbCount;
    public int OBBCount => obbCount;


    public NativeArray<ColliderSphereStruct> SphereColliders;
    private int sphereCount;
    public int SphereCount => sphereCount;



    private void Awake()
    {
        Instance = this;

        // Fetch all AudioColliders in Scene
        colliders = this.FindObjectsOfType<AudioCollider>().ToList();
        int colliderCount = colliders.Count;

        // Get total amount of colliders for box (AABB, OBB) and spheres
        for (int i = 0; i < colliderCount; i++)
        {
            aabbCount += colliders[i] as AudioAABBCollider != null ? 1 : 0;
            obbCount += colliders[i] as AudioOBBCollider != null ? 1 : 0;
            sphereCount += colliders[i] as AudioSphereCollider != null ? 1 : 0;
        }

        // Setup native arrays for colliders
        AABBColliders = new NativeArray<ColliderAABBStruct>(aabbCount, Allocator.Persistent);
        OBBColliders = new NativeArray<ColliderOBBStruct>(obbCount, Allocator.Persistent);
        SphereColliders = new NativeArray<ColliderSphereStruct>(sphereCount, Allocator.Persistent);

        for (int i = 0; i < colliderCount; i++)
        {
            CheckForResizing();
            colliders[i].AddToAudioSystem(ref AABBColliders, ref aabbCount, ref OBBColliders, ref obbCount, ref SphereColliders, ref sphereCount, i);
        }
    }

    public void AddColiderToSystem(AudioCollider targetCollider)
    {
        CheckForResizing();

        targetCollider.AddToAudioSystem(ref AABBColliders, ref aabbCount, ref OBBColliders, ref obbCount, ref SphereColliders, ref sphereCount, colliders.Count);
        colliders.Add(targetCollider);
    }

    private void CheckForResizing()
    {
        if (AABBCount == AABBColliders.Length)
        {
            ResizeDoubleCapacity(ref AABBColliders, Allocator.Persistent);
        }
        if (OBBCount == OBBColliders.Length)
        {
            ResizeDoubleCapacity(ref OBBColliders, Allocator.Persistent);
        }
        if (sphereCount == SphereColliders.Length)
        {
            ResizeDoubleCapacity(ref SphereColliders, Allocator.Persistent);
        }
    }
    private void ResizeDoubleCapacity<T>(ref NativeArray<T> array, Allocator allocator) where T : unmanaged
    {
        int newLength = math.min(array.Length * 2, 1);

        NativeArray<T> newArray = new NativeArray<T>(newLength, allocator, NativeArrayOptions.UninitializedMemory);
        NativeArray<T>.Copy(array, newArray, math.min(array.Length, newLength));
        array.Dispose();
        array = newArray;
    }

    private void OnDestroy()
    {
        AABBColliders.DisposeIfCreated();
        OBBColliders.DisposeIfCreated();
        SphereColliders.DisposeIfCreated();
    }


#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField] private bool drawColliderGizmos = true;
    [SerializeField] private Color colliderColor = new Color(1f, 0.75f, 0.25f);

    private void OnDrawGizmos()
    {
        AudioCollider[] colliders = this.FindObjectsOfType<AudioCollider>();

        //green blue-ish color
        Gizmos.color = colliderColor;

        // Draw all colliders in the collider arrays
        if (drawColliderGizmos)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].DrawColliderGizmo();
            }
        }
    }
#endif
}