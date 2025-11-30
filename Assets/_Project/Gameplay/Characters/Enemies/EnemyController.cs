using UnityEngine;
using UnityEngine.AI;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Handles enemy movement using NavMesh.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        private NavMeshAgent _agent;
        private Vector3 _targetPosition;

        public bool HasReachedDestination => _agent != null && !_agent.pathPending && _agent.remainingDistance < 0.1f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = walkSpeed;
            _agent.angularSpeed = rotationSpeed * 60f; // Convert to degrees per second
        }

        public void SetDestination(Vector3 position)
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _targetPosition = position;
                _agent.SetDestination(position);
            }
        }

        public void SetRunning(bool isRunning)
        {
            if (_agent != null)
            {
                _agent.speed = isRunning ? runSpeed : walkSpeed;
            }
        }

        public void Stop()
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
            }
        }

        public void Resume()
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = false;
            }
        }

        public void LookAt(Vector3 position)
        {
            var direction = (position - transform.position).normalized;
            direction.y = 0f; // Keep rotation on horizontal plane

            if (direction.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}

