using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct AudioRaytracerJobBatched : IJobParallelForBatch
{
    #region Raytracer Job Settings

    [ReadOnly][NoAlias] public float3 RayOrigin;
    [ReadOnly][NoAlias] public NativeArray<half3> RayDirections;

    [ReadOnly][NoAlias] public NativeArray<ColliderAABBStruct> AABBColliders;
    [ReadOnly][NoAlias] public int AABBColliderCount;
    [ReadOnly][NoAlias] public NativeArray<ColliderOBBStruct> OBBColliders;
    [ReadOnly][NoAlias] public int OBBColliderCount;
    [ReadOnly][NoAlias] public NativeArray<ColliderSphereStruct> SphereColliders;
    [ReadOnly][NoAlias] public int SphereColliderCount;

    [ReadOnly][NoAlias] public NativeArray<float3> AudioTargetPositions;
    [ReadOnly][NoAlias] public int TotalAudioTargets;

    [ReadOnly][NoAlias] public float MaxRayLife;
    [ReadOnly][NoAlias] public int MaxRayHits;

    #endregion


    #region Global Raycast data (single return arrays)

    [NativeDisableParallelForRestriction]
    [WriteOnly][NoAlias] public NativeArray<AudioRayResult> RayResults;

    [NativeDisableParallelForRestriction]
    [WriteOnly][NoAlias] public NativeArray<byte> ResultCounts;

    [NativeDisableParallelForRestriction]
    [WriteOnly][NoAlias] public NativeArray<half3> EchoRayDirections;

    #endregion


    #region Per AudioTarget Data (flattened 2D arrays based on TotalAudioTargets)

    [NativeDisableParallelForRestriction]
    [NoAlias] public NativeArray<ushort> MuffleRayHits;

    [NativeDisableParallelForRestriction]
    [NoAlias] public NativeArray<half> PermeationStrengthRemains;

    #endregion


    private const float EPSILON = 0.0001f;


    [BurstCompile]
    public void Execute(int rayStartIndex, int totalRays)
    {
        int batchCount = MuffleRayHits.Length / TotalAudioTargets;
        int batchId = rayStartIndex * batchCount / RayDirections.Length;

        //save local copy of RayOrigin
        float3 cRayOrigin;

        float rayHitDist;
        AudioRayResult rayResult;

        ColliderType hitColliderType;

        ColliderAABBStruct hitAABB = new ColliderAABBStruct();
        ColliderOBBStruct hitOBB = new ColliderOBBStruct();
        ColliderSphereStruct hitSphere = new ColliderSphereStruct();


        #region Reset Array data

        // Reset array data completely before starting
        for (int i = 0; i < totalRays * MaxRayHits; i++)
        {
            int rayIndex = rayStartIndex + i;

            EchoRayDirections[rayIndex] = half3.zero;
            RayResults[rayIndex] = new AudioRayResult();
        }

        // Reset muffleRayHit count assigned to this batch
        for (short i = 0; i < TotalAudioTargets; i++)
        {
            MuffleRayHits[batchId * TotalAudioTargets + i] = 0;
        }

        #endregion


        // 1 batch is "totalRays" amount of rays, rayStartIndex as starting index 
        for (int localRayId = 0; localRayId < totalRays; localRayId++)
        {
            int rayIndex = rayStartIndex + localRayId;

            float3 cRayDir = RayDirections[rayIndex];
            cRayOrigin = RayOrigin;

            byte cRayHits = 0;
            int rayResultId;
            float cRayLife = MaxRayLife;
            bool isRayAlive = true;


            // Loop while ray has bounces and lifetime left
            while (isRayAlive)
            {
                // Intersection tests for environment ray: AABB, OBB, Sphere
                // If a collider was hit (aka. the ray didnt go out of bounds)
                if (ShootRayCast(cRayOrigin, cRayDir, out rayResult, out hitColliderType, out rayHitDist, out hitAABB, out hitOBB, out hitSphere))
                {
                    // Update new ray origin, ray totalDist and add 1 bounce
                    cRayOrigin += cRayDir * rayHitDist;
                    cRayLife -= rayHitDist;
                    cRayHits += 1;

                    rayResultId = rayIndex * MaxRayHits +cRayHits - 1;
#if UNITY_EDITOR
                    //for debugging like drawing gizmos
                    rayResult.DEBUG_HitPoint = (half3)cRayOrigin;
#endif


                    #region Check if hit ray point can return to original origin point (Blue Echo rays to player)

                    float3 offsettedRayHitWorldPoint = cRayOrigin - cRayDir * EPSILON; //offset the hit point a bit so it doesnt intersect with same collider again

                    //shoot a return ray to the original origin
                    float3 returnRayDir = math.normalize(RayOrigin - offsettedRayHitWorldPoint);

                    //calculate the distance to the origin and offset RayOrigin by a bit back so it doesnt intersect with same collider again
                    float distToOriginalOrigin = math.distance(RayOrigin, offsettedRayHitWorldPoint);

                    // if nothing was hit, aka the ray go to the player succesfully store the return ray direction
                    if (CanRaySeePoint(offsettedRayHitWorldPoint, returnRayDir, distToOriginalOrigin))
                    {
                        EchoRayDirections[rayResultId] = (half3)returnRayDir;
                    }
                
                    #endregion


                    #region Check if ray can get to audiotarget (Green Muffle rays to all audio targets)

                    // Raycast to each AudioTarget position
                    for (short i = 0; i < TotalAudioTargets; i++)
                    {
                        int muffleRayId = batchId * TotalAudioTargets + i;

                        // Offset the hit point a bit so it doesnt intersect with same collider again
                        offsettedRayHitWorldPoint = cRayOrigin - cRayDir * EPSILON;

                        // Get Position of current AudioTarget and direction towards it with cRayOrigin
                        float3 audioTargetPosition = AudioTargetPositions[i];
                        float3 rayToTargetDir = math.normalize(audioTargetPosition - offsettedRayHitWorldPoint);

                        // Calculate distance to the audio target
                        float distToTarget = math.distance(offsettedRayHitWorldPoint, audioTargetPosition);

                        // Cast a ray from the hit point to the audio target
                        if (CanRaySeeAudioTarget(offsettedRayHitWorldPoint, rayToTargetDir, distToTarget, i))
                        {
                            // If the ray to the audio target is clear, increment the appropriate entry in MuffleRayHits
                            MuffleRayHits[muffleRayId] += 1;

                            //rayResult.AudioTargetId = i; // Set the audio target Id in the result
                        }
                    }

                    #endregion


                    // Check if ray is finished (if rayHits is more than MaxRayHits or totalDist is equal or exceeds MaxRayDist)
                    if (cRayHits >= MaxRayHits || cRayLife <= 0)
                    {
                        // If ray dies this iteration, give it the totalDist traveled value as its fullRayDistance
                        rayResult.FullRayDistance = (half)(MaxRayLife - cRayLife);

                        isRayAlive = false; // Ray wont bounce another time.
                    }
                    else
                    {
                        // If ray is still alive, update next ray direction and origin (bouncing it of the hit normal),
                        // also get soundAbsorption stat from hit wall
                        ReflectRay(hitColliderType, hitAABB, hitOBB, hitSphere, ref cRayOrigin, ref cRayDir, ref cRayLife);
                    }

                    // Add hit result to return data array
                    RayResults[rayResultId] = rayResult;
                }
                else
                {
                    ResultCounts[rayIndex] = cRayHits;
                    break; // Ray went out of bounds (Didnt hit anything), kill ray instantly
                }
            }

            // Ray ended, write total ray hits into ResultCounts
            ResultCounts[rayIndex] = cRayHits;
        }
    }


    #region Collider Intersection Checks (The Actual Raycasting Part)

    /// <summary>
    /// Checks all colliders for intersection with the ray and returns the closest hit collider and its type, data, and distance.
    /// </summary>
    /// <returns>True if the ray hits any collider; otherwise, false.</returns>
    [BurstCompile]
    private bool ShootRayCast(float3 cRayOrigin, float3 cRayDir,
        out AudioRayResult rayResult, out ColliderType hitColliderType, out float closestDist,
        out ColliderAABBStruct hitAABB, out ColliderOBBStruct hitOBB, out ColliderSphereStruct hitSphere)
    {
        float dist;
        closestDist = float.MaxValue;
        rayResult = new AudioRayResult();

        hitColliderType = ColliderType.None;
        hitAABB = new ColliderAABBStruct();
        hitOBB = new ColliderOBBStruct();
        hitSphere = new ColliderSphereStruct();

        // Box intersections (AABB)
        for (int i = 0; i < AABBColliderCount; i++)
        {
            var tempAABB = AABBColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsAABB(cRayOrigin, cRayDir, tempAABB.center, tempAABB.size, out dist) && dist < closestDist)
            {
                hitColliderType = ColliderType.AABB;
                hitAABB = tempAABB;
                closestDist = dist;
            }
        }
        // Rotated box intersections (OBB)
        for (int i = 0; i < OBBColliderCount; i++)
        {
            var tempOBB = OBBColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsOBB(cRayOrigin, cRayDir, tempOBB.center, tempOBB.size, tempOBB.Rotation, out dist) && dist < closestDist)
            {
                hitColliderType = ColliderType.OBB;
                hitOBB = tempOBB;
                closestDist = dist;
            }
        }
        // Sphere intersections
        for (int i = 0; i < SphereColliderCount; i++)
        {
            var tempSphere = SphereColliders[i];

            // If collider is hit AND it is the closest hit so far
            if (RayIntersectsSphere(cRayOrigin, cRayDir, tempSphere.center, tempSphere.radius, out dist) && dist < closestDist)
            {
                hitColliderType = ColliderType.Sphere;
                hitSphere = tempSphere;
                closestDist = dist;
            }
        }

        rayResult.Distance = (half)closestDist;

        // Return whether a hit was detected
        return hitColliderType != ColliderType.None;
    }


    [BurstCompile]
    private bool RayIntersectsAABB(float3 rayOrigin, float3 rayDir, float3 center, float3 halfExtents, out float distance)
    {
        float3 min = center - halfExtents;
        float3 max = center + halfExtents;

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
    private bool RayIntersectsOBB(float3 rayOrigin, float3 rayDir, float3 center, float3 halfExtents, quaternion rotation, out float distance)
    {
        quaternion invRotation = math.inverse(rotation);
        float3 localOrigin = math.mul(invRotation, rayOrigin - center);
        float3 localDir = math.mul(invRotation, rayDir);

        return RayIntersectsAABB(localOrigin, localDir, float3.zero, halfExtents, out distance);
    }


    [BurstCompile]
    private bool RayIntersectsSphere(float3 rayOrigin, float3 rayDir, float3 center, float radius, out float distance)
    {
        float3 oc = rayOrigin - center;
        float a = math.dot(rayDir, rayDir);
        float b = 2.0f * math.dot(oc, rayDir);
        float c = math.dot(oc, oc) - radius * radius;
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


    /// <summary>
    /// Check if world point is visible from the ray origin, meaning no colliders are in the way.
    /// </summary>
    [BurstCompile]
    private bool CanRaySeePoint(float3 rayOrigin, float3 rayDir, float distToTarget)
    {
        float dist;

        //check against AABBs
        for (int i = 0; i < AABBColliderCount; i++)
        {
            var tempAABB = AABBColliders[i];
            if (RayIntersectsAABB(rayOrigin, rayDir, tempAABB.center, tempAABB.size, out dist) && dist < distToTarget)
            {
                return false;
            }
        }
        //if there was no AABB hit, check against OBBs
        for (int i = 0; i < OBBColliderCount; i++)
        {
            var tempOBB = OBBColliders[i];
            if (RayIntersectsOBB(rayOrigin, rayDir, tempOBB.center, tempOBB.size, tempOBB.Rotation, out dist) && dist < distToTarget)
            {
                return false;
            }
        }
        //if there were no AABB and OBB hits, check against spheres
        for (int i = 0; i < SphereColliderCount; i++)
        {
            var tempSphere = SphereColliders[i];
            if (RayIntersectsSphere(rayOrigin, rayDir, tempSphere.center, tempSphere.radius, out dist) && dist < distToTarget)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Identical to CanRaySeePoint, but skips hits against the colliders of the audioTarget
    /// </summary>
    [BurstCompile]
    private bool CanRaySeeAudioTarget(float3 rayOrigin, float3 rayDir, float distToOriginalOrigin, int audioTargetId)
    {
        //check against AABBs
        for (int i = 0; i < AABBColliderCount; i++)
        {
            var collider = AABBColliders[i];

            //skip colliders that belong to the audiotarget, since we otherwise are unable to get to audioTargetPosition
            if (collider.audioTargetId == audioTargetId)
            {
                continue;
            }

            if (RayIntersectsAABB(rayOrigin, rayDir, collider.center, collider.size, out float dist) && dist < distToOriginalOrigin)
            {
                return false;
            }
        }
        //if there was no AABB hit, check against OBBs
        for (int i = 0; i < OBBColliderCount; i++)
        {
            var collider = OBBColliders[i];

            //skip colliders that belong to the audiotarget, since we otherwise are unable to get to audioTargetPosition
            if (collider.audioTargetId == audioTargetId)
            {
                continue;
            }

            if (RayIntersectsOBB(rayOrigin, rayDir, collider.center, collider.size, collider.Rotation, out float dist) && dist < distToOriginalOrigin)
            {
                return false;
            }
        }
        //if there were no AABB and OBB hits, check against spheres
        for (int i = 0; i < SphereColliderCount; i++)
        {
            var collider = SphereColliders[i];

            //skip colliders that belong to the audiotarget, since we otherwise are unable to get to audioTargetPosition
            if (collider.audioTargetId == audioTargetId)
            {
                continue;
            }

            if (RayIntersectsSphere(rayOrigin, rayDir, collider.center, collider.radius, out float dist) && dist < distToOriginalOrigin)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Calculate the new ray direction and origin after a hit, based on the hit collider type, so it "bounces" of the hits surface. Also subtract rayLife by aborption value of hit collider.
    /// </summary>
    [BurstCompile]
    private void ReflectRay(ColliderType hitColliderType, ColliderAABBStruct hitAABB, ColliderOBBStruct hitOBB, ColliderSphereStruct hitSphere, ref float3 cRayOrigin, ref float3 cRayDir, ref float cRayLife)
    {
        float3 normal = float3.zero;
        float absorption = 0;

        switch (hitColliderType)
        {
            case ColliderType.AABB:

                float3 localPoint = cRayOrigin - hitAABB.center;
                float3 absPoint = math.abs(localPoint);
                float3 halfExtents = hitAABB.size;
                normal = float3.zero;

                // Reflect the ray based on the closest axis
                if (halfExtents.x - absPoint.x < halfExtents.y - absPoint.y && halfExtents.x - absPoint.x < halfExtents.z - absPoint.z)
                {
                    normal.x = math.sign(localPoint.x);
                }
                else if (halfExtents.y - absPoint.y < halfExtents.x - absPoint.x && halfExtents.y - absPoint.y < halfExtents.z - absPoint.z)
                {
                    normal.y = math.sign(localPoint.y);
                }
                else
                {
                    normal.z = math.sign(localPoint.z);
                }

                absorption = hitAABB.absorptionValue;
                break;

            case ColliderType.OBB:

                float3 localHit = math.mul(math.inverse(hitOBB.Rotation), cRayOrigin - hitOBB.center);
                float3 localHalfExtents = hitOBB.size;

                float3 absPointOBB = math.abs(localHit);
                float3 deltaToFaceOBB = localHalfExtents - absPointOBB;

                float3 localNormal = float3.zero;

                if (deltaToFaceOBB.x < deltaToFaceOBB.y && deltaToFaceOBB.x < deltaToFaceOBB.z)
                {
                    localNormal.x = math.sign(localHit.x);
                }
                else if (deltaToFaceOBB.y < deltaToFaceOBB.x && deltaToFaceOBB.y < deltaToFaceOBB.z)
                {
                    localNormal.y = math.sign(localHit.y);
                }
                else
                {
                    localNormal.z = math.sign(localHit.z);
                }

                normal = math.mul(hitOBB.Rotation, localNormal);
                absorption = hitOBB.absorptionValue;
                break;

            case ColliderType.Sphere:

                normal = math.normalize(cRayOrigin - hitSphere.center);
                absorption = hitSphere.absorptionValue;
                break;

            default:
                break;
        }

        // Update next ray direction (bouncing it of the hit wall)
        cRayDir = math.reflect(cRayDir, normal);

        // Update rays new origin (hit point)
        cRayOrigin += cRayDir * EPSILON;

        // Drain raylife based on hit collider absorption
        cRayLife -= absorption;
    }
}
