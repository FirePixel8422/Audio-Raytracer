using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class AudioMaterialInitializer : MonoBehaviour
{
    public static AudioMaterialInitializer Instance { get; private set; }
    // KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_
    // KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_KILL_INSTANCE_


    [SerializeField] private AudioMaterialPropertiesSO[] materialTypeConfigs;


    [Tooltip("How much power of the mainRays hitting this surface gets consumed")]
    public NativeArray<half> Absorption;

    [Tooltip("How much power gets consumed when permeation rays go through material")]
    public NativeArray<half> TransmissionLoss;

    [Tooltip("How much reflected sound is scattered away from the perfect reflection direction")]
    public NativeArray<half> Scattering;

    [Tooltip("Echo power multiplier when an echo ray hits this surface")]
    public NativeArray<half> Echo;


    private void Awake()
    {
        Instance = this;

        int materialCount = materialTypeConfigs.Length;

        Absorption = new NativeArray<half>(materialCount, Allocator.Persistent);
        TransmissionLoss = new NativeArray<half>(materialCount, Allocator.Persistent);
        Scattering = new NativeArray<half>(materialCount, Allocator.Persistent);
        Echo = new NativeArray<half>(materialCount, Allocator.Persistent);

        for (short i = 0; i < materialCount; i++)
        {
            materialTypeConfigs[i].Id = i;

            Absorption[i] = materialTypeConfigs[i].MaterialProperties.Absorption;
            TransmissionLoss[i] = materialTypeConfigs[i].MaterialProperties.TransmissionLoss;
            Scattering[i] = materialTypeConfigs[i].MaterialProperties.Scattering;
            Echo[i] = materialTypeConfigs[i].MaterialProperties.Echo;
        }
    }

    private void OnDestroy()
    {
        Absorption.Dispose();
        TransmissionLoss.Dispose();
        Scattering.Dispose();
        Echo.Dispose(); 
    }
}