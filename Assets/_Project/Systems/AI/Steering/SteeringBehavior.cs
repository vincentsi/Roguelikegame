using UnityEngine;

namespace ProjectRoguelike.AI.Steering
{
    /// <summary>
    /// Steering behaviors for AI movement (pursuit, avoidance, etc.)
    /// </summary>
    public static class SteeringBehavior
    {
        /// <summary>
        /// Pursuit: predict where target will be and move towards that position.
        /// </summary>
        public static Vector3 Pursuit(Vector3 position, Vector3 targetPosition, Vector3 targetVelocity, float maxSpeed)
        {
            var distance = Vector3.Distance(position, targetPosition);
            var predictionTime = distance / maxSpeed;
            var predictedPosition = targetPosition + targetVelocity * predictionTime;
            return Seek(position, predictedPosition, maxSpeed);
        }

        /// <summary>
        /// Seek: move directly towards target.
        /// </summary>
        public static Vector3 Seek(Vector3 position, Vector3 target, float maxSpeed)
        {
            var desired = (target - position).normalized * maxSpeed;
            return desired;
        }

        /// <summary>
        /// Flee: move away from target.
        /// </summary>
        public static Vector3 Flee(Vector3 position, Vector3 target, float maxSpeed)
        {
            var desired = (position - target).normalized * maxSpeed;
            return desired;
        }

        /// <summary>
        /// Avoidance: avoid obstacles or other entities.
        /// </summary>
        public static Vector3 Avoidance(Vector3 position, Vector3 forward, float avoidanceRadius, LayerMask obstacleLayer, float maxSpeed)
        {
            // Simple sphere cast forward
            if (Physics.SphereCast(position, avoidanceRadius, forward, out var hit, avoidanceRadius * 2f, obstacleLayer))
            {
                var avoidanceForce = (position - hit.point).normalized * maxSpeed;
                return avoidanceForce;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Separation: maintain distance from nearby entities.
        /// </summary>
        public static Vector3 Separation(Vector3 position, Vector3[] neighborPositions, float separationRadius, float maxSpeed)
        {
            var separationForce = Vector3.zero;
            var neighborCount = 0;

            foreach (var neighborPos in neighborPositions)
            {
                var distance = Vector3.Distance(position, neighborPos);
                if (distance > 0f && distance < separationRadius)
                {
                    var diff = (position - neighborPos).normalized / distance; // Weight by distance
                    separationForce += diff;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
                separationForce.Normalize();
                separationForce *= maxSpeed;
            }

            return separationForce;
        }

        /// <summary>
        /// Alignment: align velocity with neighbors.
        /// </summary>
        public static Vector3 Alignment(Vector3 currentVelocity, Vector3[] neighborVelocities, float maxSpeed)
        {
            if (neighborVelocities.Length == 0)
            {
                return Vector3.zero;
            }

            var averageVelocity = Vector3.zero;
            foreach (var vel in neighborVelocities)
            {
                averageVelocity += vel;
            }

            averageVelocity /= neighborVelocities.Length;
            averageVelocity.Normalize();
            averageVelocity *= maxSpeed;

            return averageVelocity - currentVelocity;
        }

        /// <summary>
        /// Cohesion: move towards center of mass of neighbors.
        /// </summary>
        public static Vector3 Cohesion(Vector3 position, Vector3[] neighborPositions, float maxSpeed)
        {
            if (neighborPositions.Length == 0)
            {
                return Vector3.zero;
            }

            var centerOfMass = Vector3.zero;
            foreach (var neighborPos in neighborPositions)
            {
                centerOfMass += neighborPos;
            }

            centerOfMass /= neighborPositions.Length;
            return Seek(position, centerOfMass, maxSpeed);
        }
    }
}

