using ProjectRoguelike.Gameplay.Combat;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Weapons
{
    public sealed class HitscanWeapon : WeaponBase
    {
        [SerializeField] private float damage = 24f;
        [SerializeField] private float range = 120f;
        [SerializeField] private float impulse = 5f;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private GameObject hitVfxPrefab;

        protected override void HandleShot(Vector3 origin, Vector3 direction)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            if (!Physics.Raycast(origin, direction, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Debug.Log($"[HitscanWeapon] Hit: {hit.collider.name} at {hit.point}");
            
            // Try to get IDamageable from the hit object or its parents
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null)
            {
                Debug.Log($"[HitscanWeapon] Applying {damage} damage to {hit.collider.name}");
                damageable.ApplyDamage(damage);
            }
            else
            {
                Debug.Log($"[HitscanWeapon] No IDamageable found on {hit.collider.name} or its parents");
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(direction * impulse, hit.point, ForceMode.Impulse);
            }

            if (hitVfxPrefab != null)
            {
                var vfx = Object.Instantiate(hitVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Object.Destroy(vfx, 2f);
            }
        }
    }
}

