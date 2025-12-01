using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.AI.GroupBehavior
{
    /// <summary>
    /// Manages groups of enemies for coordinated behavior (formation, coordination).
    /// </summary>
    public sealed class EnemyGroupManager : MonoBehaviour
    {
        [Header("Group Settings")]
        [SerializeField] private float groupRadius = 10f;
        [SerializeField] private int maxGroupSize = 5;

        private readonly Dictionary<int, List<GameObject>> _groups = new();
        private int _nextGroupId = 1;

        public int CreateGroup()
        {
            var groupId = _nextGroupId++;
            _groups[groupId] = new List<GameObject>();
            return groupId;
        }

        public void AddToGroup(int groupId, GameObject enemy)
        {
            if (!_groups.ContainsKey(groupId))
            {
                _groups[groupId] = new List<GameObject>();
            }

            if (!_groups[groupId].Contains(enemy))
            {
                _groups[groupId].Add(enemy);
            }
        }

        public void RemoveFromGroup(int groupId, GameObject enemy)
        {
            if (_groups.ContainsKey(groupId))
            {
                _groups[groupId].Remove(enemy);
                if (_groups[groupId].Count == 0)
                {
                    _groups.Remove(groupId);
                }
            }
        }

        public List<GameObject> GetGroupMembers(int groupId)
        {
            return _groups.ContainsKey(groupId) ? _groups[groupId] : new List<GameObject>();
        }

        public List<GameObject> GetNearbyEnemies(Vector3 position, float radius)
        {
            var nearby = new List<GameObject>();
            var colliders = Physics.OverlapSphere(position, radius);

            foreach (var col in colliders)
            {
                // Check if it's an enemy
                if (col.CompareTag("Enemy") && col.gameObject != gameObject)
                {
                    nearby.Add(col.gameObject);
                }
            }

            return nearby;
        }

        public Vector3 GetGroupCenter(int groupId)
        {
            if (!_groups.ContainsKey(groupId) || _groups[groupId].Count == 0)
            {
                return Vector3.zero;
            }

            var center = Vector3.zero;
            var count = 0;

            foreach (var enemy in _groups[groupId])
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    center += enemy.transform.position;
                    count++;
                }
            }

            return count > 0 ? center / count : Vector3.zero;
        }
    }
}

