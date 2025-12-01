using UnityEngine;

namespace ProjectRoguelike.AI.GroupBehavior
{
    /// <summary>
    /// Component for enemies that participate in group behavior.
    /// </summary>
    public sealed class EnemyGroupMember : MonoBehaviour
    {
        [Header("Group Settings")]
        [SerializeField] private bool useGroupBehavior = true;
        [SerializeField] private float separationRadius = 2f;
        [SerializeField] private float alignmentRadius = 5f;
        [SerializeField] private float cohesionRadius = 8f;

        private EnemyGroupManager _groupManager;
        private int _groupId = -1;

        public int GroupId => _groupId;
        public bool UseGroupBehavior => useGroupBehavior;

        private void Awake()
        {
            _groupManager = FindObjectOfType<EnemyGroupManager>();
            if (_groupManager == null)
            {
                // Create group manager if it doesn't exist
                var managerObj = new GameObject("EnemyGroupManager");
                _groupManager = managerObj.AddComponent<EnemyGroupManager>();
            }
        }

        private void Start()
        {
            if (useGroupBehavior && _groupManager != null)
            {
                // Auto-assign to a group or create new one
                _groupId = _groupManager.CreateGroup();
                _groupManager.AddToGroup(_groupId, gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_groupManager != null && _groupId >= 0)
            {
                _groupManager.RemoveFromGroup(_groupId, gameObject);
            }
        }

        public Vector3[] GetNearbyEnemyPositions()
        {
            if (_groupManager == null || _groupId < 0)
            {
                return new Vector3[0];
            }

            var members = _groupManager.GetGroupMembers(_groupId);
            var positions = new System.Collections.Generic.List<Vector3>();

            foreach (var member in members)
            {
                if (member != null && member != gameObject && member.activeInHierarchy)
                {
                    positions.Add(member.transform.position);
                }
            }

            return positions.ToArray();
        }

        public Vector3 GetGroupCenter()
        {
            if (_groupManager == null || _groupId < 0)
            {
                return transform.position;
            }

            return _groupManager.GetGroupCenter(_groupId);
        }

        public float SeparationRadius => separationRadius;
        public float AlignmentRadius => alignmentRadius;
        public float CohesionRadius => cohesionRadius;
    }
}

