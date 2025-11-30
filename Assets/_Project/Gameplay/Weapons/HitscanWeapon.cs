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

            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.ApplyDamage(damage);

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

