using UnityEngine;
using ProjectRoguelike.Gameplay.Items;
using ProjectRoguelike.Gameplay.Weapons;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// ScriptableObject containing weapon stats and configuration.
    /// Used to create weapon variants with different stats.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData_", menuName = "Roguelike/Weapon Data", order = 1)]
    public sealed class WeaponData : ItemData
    {
        [Header("Weapon Prefab")]
        [SerializeField] private WeaponBase weaponPrefab;

        [Header("Weapon Stats")]
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int ammoReserve = 90;
        [SerializeField] private float fireRate = 10f;
        [SerializeField] private float reloadDuration = 1.6f;
        [SerializeField] private Vector2 recoilKick = new Vector2(0.2f, 0.3f);

        [Header("Hitscan Stats (if applicable)")]
        [SerializeField] private float damage = 24f;
        [SerializeField] private float range = 120f;
        [SerializeField] private float impulse = 5f;

        public WeaponBase WeaponPrefab => weaponPrefab;
        public int MagazineSize => magazineSize;
        public int AmmoReserve => ammoReserve;
        public float FireRate => fireRate;
        public float ReloadDuration => reloadDuration;
        public Vector2 RecoilKick => recoilKick;
        public float Damage => damage;
        public float Range => range;
        public float Impulse => impulse;

        public override void OnPickup(GameObject player)
        {
            var weaponController = player.GetComponent<Weapons.WeaponController>();
            if (weaponController == null)
            {
                Debug.LogWarning($"[WeaponData] Player {player.name} has no WeaponController!");
                return;
            }

            if (weaponPrefab == null)
            {
                Debug.LogError($"[WeaponData] WeaponPrefab is null for {ItemName}!");
                return;
            }

            // Create weapon instance from prefab
            var weaponInstance = Instantiate(weaponPrefab);
            
            // Apply weapon data stats to the weapon instance
            ApplyWeaponData(weaponInstance);
            
            // Equip the weapon
            weaponController.Equip(weaponInstance);
        }

        private void ApplyWeaponData(WeaponBase weapon)
        {
            if (weapon == null)
            {
                return;
            }

            weapon.ApplyWeaponData(this);
        }
    }
}

