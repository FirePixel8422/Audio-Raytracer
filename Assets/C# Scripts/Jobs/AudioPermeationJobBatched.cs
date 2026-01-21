using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct AudioPermeationJobBatched : IJobParallelForBatch
{
    [ReadOnly, NoAlias] public float3 RayOrigin;
    [ReadOnly, NoAlias] public NativeArray<half3> RayDirections;
             
    [ReadOnly, NoAlias] public NativeArray<ColliderAABBStruct> AABBColliders;
    [ReadOnly, NoAlias] public int AABBColliderCount;
    [ReadOnly, NoAlias] public NativeArray<ColliderOBBStruct> OBBColliders;
    [ReadOnly, NoAlias] public int OBBColliderCount;
    [ReadOnly, NoAlias] public NativeArray<ColliderSphereStruct> SphereColliders;
    [ReadOnly, NoAlias] public int SphereColliderCount;
             
    [ReadOnly, NoAlias] public NativeArray<float3> AudioTargetPositions;
    [ReadOnly, NoAlias] public int TotalAudioTargets;

    [ReadOnly, NoAlias] public float PermeationStrengthPerRay;


    [NativeDisableParallelForRestriction]
    [WriteOnly, NoAlias] public NativeArray<float> PermeationPowerRemains;


    private const float EPSILON = 0.0001f;


    [BurstCompile]
    public void Execute(int rayStartIndex, int totalRays)
    {
        int batchCount = PermeationPowerRemains.Length / totalRays / TotalAudioTargets;
        int batchId = rayStartIndex * batchCount / RayDirections.Length;

        // Save local copy of RayOrigin
        float3 cRayOrigin;

        // Reset permeation assigned to this batch
        for (short i = 0; i < TotalAudioTargets; i++)
        {
            PermeationPowerRemains[batchId * TotalAudioTargets + i] = (half)0;
        }

        // 1 batch is "totalRays" amount of rays, rayStartIndex as starting index 
        for (int localRayId = 0; localRayId < totalRays; localRayId++)
        {
            int rayIndex = rayStartIndex + localRayId;

            float3 cRayDir = RayDirections[rayIndex];
            cRayOrigin = RayOrigin;


            // Get first intersection point and start permeation checks from there
            if (ShootRayCast(cRayOrigin, cRayDir, out float rayHitDist))
            {
                // Update new ray origin
                cRayOrigin += cRayDir * rayHitDist;


                #region Check if ray can get to audiotarget (Muffle rays to all audio targets)

                // Raycast to each AudioTarget position
                for (short AudioTargetId = 0; AudioTargetId < TotalAudioTargets; AudioTargetId++)
                {
                    int permeationRayId = batchId * TotalAudioTargets + AudioTargetId;

                    // Offset the hit point a bit so it doesnt intersect with same collider again
                    float3 offsettedRayHitWorldPoint = cRayOrigin - cRayDir * EPSILON;

                    // Get Position of current AudioTarget and direction towards it with cRayOrigin
                    float3 audioTargetPosition = AudioTargetPositions[AudioTargetId];
                    float3 rayToTargetDir = math.normalize(audioTargetPosition - offsettedRayHitWorldPoint);

                    // Calculate distance to the audio target
                    float distToTarget = math.distance(offsettedRayHitWorldPoint, audioTargetPosition);

                    // Get permeation power remains after checking all colliders along the ray
                    ShootPermeationRayCast(offsettedRayHitWorldPoint, rayToTargetDir, distToTarget, AudioTargetId, out float permeationPowerRemains);

                    // Write it back to return array
                    PermeationPowerRemains[permeationRayId] = permeationPowerRemains;
                }

                #endregion
            }
        }
    }


    #region Collider Intersection Checks (The Actual Raycasting Part)

    /// <summary>
    /// Checks all colliders for intersection with the ray and returns the closest hit distance.
    /// </summary>
    /// <returns>True if the ray hits any collider; otherwise, false.</returns>
    [BurstCompile]
    private bool ShootRayCast(float3 cRayOrigin, float3 cRayDir, out float closestDist)
    {
        float dist;
        closestDist = math.INFINITY;
        // Sphere intersections
        for (int i = 0; i < SphereColliderCount; i++)
        {
            var tempSphere = SphereColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsSphere(cRayOrigin, cRayDir, tempSphere.Center, tempSphere.Radius, out dist) && dist < closestDist)
            {
                closestDist = dist;
            }
        }
        // Box intersections (AABB)
        for (int i = 0; i < AABBColliderCount; i++)
        {
            var tempAABB = AABBColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsAABB(cRayOrigin, cRayDir, tempAABB.Center, tempAABB.Size, out dist) && dist < closestDist)
            {
                closestDist = dist;
            }
        }
        // Rotated Box intersections (OBB)
        for (int i = 0; i < OBBColliderCount; i++)
        {
            var tempOBB = OBBColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsOBB(cRayOrigin, cRayDir, tempOBB.Center, tempOBB.Size, tempOBB.Rotation, out dist) && dist < closestDist)
            {
                closestDist = dist;
            }
        }

        // Return whether a hit was detected
        return closestDist != math.INFINITY;
    }


    [BurstCompile]
    private bool RayIntersectsAABB(float3 rayOrigin, float3 rayDir, float3 Center, float3 halfExtents, out float distance)
    {
        float3 min = Center - halfExtents;
        float3 max = Center + halfExtents;

        float3 invDir = 1.0f / rayDir;

        float3 t0 = (min - rayOrigin) * invDir;
        float3 t1 = (max - rayOrigin) * invDir;

        float3 tmin = math.min(t0, t1);
        float3 tmax = math.max(t0, t1);

        float tNear = math.max(math.max(tmin.x, tmin.y), tmin.z);
        float tFar = math.min(math.min(tmax.x, tmax.y), tmax.z);

        if (tNear > tFar || tFar < 0)
        {
            distance = 0;
            return false;
        }

        distance = tNear > 0 ? tNear : tFar;
        return true;
    }

    [BurstCompile]
    private bool RayIntersectsOBB(float3 rayOrigin, float3 rayDir, float3 Center, float3 halfExtents, quaternion rotation, out float distance)
    {
        quaternion invRotation = math.inverse(rotation);
        float3 localOrigin = math.mul(invRotation, rayOrigin - Center);
        float3 localDir = math.mul(invRotation, rayDir);

        return RayIntersectsAABB(localOrigin, localDir, float3.zero, halfExtents, out distance);
    }

    [BurstCompile]
    private bool RayIntersectsSphere(float3 rayOrigin, float3 rayDir, float3 Center, float Radius, out float distance)
    {
        float3 oc = rayOrigin - Center;
        float a = math.dot(rayDir, rayDir);
        float b = 2.0f * math.dot(oc, rayDir);
        float c = math.dot(oc, oc) - Radius * Radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            distance = 0;
            return false;
        }

        float sqrtDiscriminant = math.sqrt(discriminant);
        float t0 = (-b - sqrtDiscriminant) / (2.0f * a);
        float t1 = (-b + sqrtDiscriminant) / (2.0f * a);

        // Select the nearest valid intersection
        if (t0 >= 0)
        {
            distance = t0;
            return true;
        }
        else if (t1 >= 0)
        {
            distance = t1;
            return true;
        }

        distance = 0;
        return false;
    }

    #endregion


    #region Permeation Intersection Distance Checks

    /// <summary>
    /// Get permeation power remains after checking all colliders along the ray.
    /// </summary>
    [BurstCompile]
    private void ShootPermeationRayCast(float3 rayOrigin, float3 rayDir, float distToStartOrigin, int AudioTargetId, out float permeationPowerRemains)
    {
        float totalPermeationPowerLoss = 0;

        // Check against Spheres
        for (int i = 0; i < SphereColliderCount; i++)
        {
            var tempSphere = SphereColliders[i];

            // Skip colliders that belong to the audiotarget
            if (tempSphere.AudioTargetId == AudioTargetId) continue;

            RayIntersectsSpherePermeation(rayOrigin, rayDir, tempSphere.Center, tempSphere.Radius, tempSphere.MaterialProperties.Density, ref totalPermeationPowerLoss);
        }
        // Check against AABBs
        for (int i = 0; i < AABBColliderCount; i++)
        {
            var tempAABB = AABBColliders[i];

            // Skip colliders that belong to the audiotarget
            if (tempAABB.AudioTargetId == AudioTargetId) continue;

            RayIntersectsAABBPermeation(rayOrigin, rayDir, tempAABB.Center, tempAABB.Size, tempAABB.MaterialProperties.Density, ref totalPermeationPowerLoss);
        }
        // Check against OBBs
        for (int i = 0; i < OBBColliderCount; i++)
        {
            var tempOBB = OBBColliders[i];

            // Skip colliders that belong to the audiotarget
            if (tempOBB.AudioTargetId == AudioTargetId) continue;

            RayIntersectsOBBPermeation(rayOrigin, rayDir, tempOBB.Center, tempOBB.Size, tempOBB.Rotation, tempOBB.MaterialProperties.Density, ref totalPermeationPowerLoss);
        }

        permeationPowerRemains = RayDirections.Length * PermeationStrengthPerRay - totalPermeationPowerLoss;
    }


    [BurstCompile]
    private void RayIntersectsAABBPermeation(float3 rayOrigin, float3 rayDir, float3 Center, float3 halfExtents, float densityMultiplier, ref float totalPermeationPowerLoss)
    {
        float3 min = Center - halfExtents;
        float3 max = Center + halfExtents;

        float3 invDir = 1.0f / rayDir;

        float3 t0 = (min - rayOrigin) * invDir;
        float3 t1 = (max - rayOrigin) * invDir;

        float3 tmin = math.min(t0, t1);
        float3 tmax = math.max(t0, t1);

        float tEnter = math.max(math.max(tmin.x, tmin.y), tmin.z);
        float tExit = math.min(math.min(tmax.x, tmax.y), tmax.z);

        if (tEnter > tExit || tExit < 0f)
        {
            return;
        }

        float enter = math.max(tEnter, 0f);
        totalPermeationPowerLoss += math.max(0f, tExit - enter) * densityMultiplier;
    }

    /// <summary>
    /// OBB Colliders have their "Rotation" pre-calculated as inverse quaternion for optimization.
    /// </summary>
    [BurstCompile]
    private void RayIntersectsOBBPermeation(float3 rayOrigin, float3 rayDir, float3 Center, float3 halfExtents, quaternion invRotation, float densityMultiplier, ref float totalPermeationPowerLoss)
    {
        float3 localOrigin = math.mul(invRotation, rayOrigin - Center);
        float3 localDir = math.mul(invRotation, rayDir);

        RayIntersectsAABBPermeation(localOrigin, localDir, float3.zero, halfExtents, densityMultiplier, ref totalPermeationPowerLoss);
    }

    [BurstCompile]
    private void RayIntersectsSpherePermeation(float3 rayOrigin, float3 rayDir, float3 Center, float Radius, float densityMultiplier, ref float totalPermeationPowerLoss)
    {
        float3 oc = rayOrigin - Center;

        float b = math.dot(oc, rayDir);
        float c = math.dot(oc, oc) - Radius * Radius;
        float discriminant = b * b - c;

        if (discriminant < 0f)
        {
            return;
        }

        float sqrtD = math.sqrt(discriminant);

        float tEnter = -b - sqrtD;
        float tExit = -b + sqrtD;

        if (tExit < 0f)
        {
            return;
        }

        float enter = math.max(tEnter, 0f);
        totalPermeationPowerLoss += math.max(0f, tExit - enter) * densityMultiplier;
    }

    #endregion
}
