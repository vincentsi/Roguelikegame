using ProjectRoguelike.Gameplay.Combat;
using ProjectRoguelike.AI.Sensors;
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
        [SerializeField] private WeaponAudio weaponAudio;
        [SerializeField] private float gunshotSoundIntensity = 1f; // How loud the gunshot is

        protected override void HandleShot(Vector3 origin, Vector3 direction)
        {
            // Play muzzle flash
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            // Play fire sound
            if (weaponAudio != null)
            {
                weaponAudio.PlayFireSound();
            }

            // Notify all hearing sensors about the gunshot
            NotifyHearingSensors(origin);

            if (!Physics.Raycast(origin, direction, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            // Try to get IDamageable from the hit object or its parents
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null)
            {
                damageable.ApplyDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(direction * impulse, hit.point, ForceMode.Impulse);
            }

            // Spawn hit VFX
            if (hitVfxPrefab != null)
            {
                var vfx = Object.Instantiate(hitVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                var particleSystem = vfx.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
                Object.Destroy(vfx, 2f);
            }

            // Play hit sound
            if (weaponAudio != null)
            {
                weaponAudio.PlayHitSound();
            }
        }

        public override void ApplyWeaponData(Items.WeaponData data)
        {
            base.ApplyWeaponData(data);
            
            if (data == null)
            {
                return;
            }

            damage = data.Damage;
            range = data.Range;
            impulse = data.Impulse;
        }

        private void NotifyHearingSensors(Vector3 soundPosition)
        {
            // Find all hearing sensors in the scene and notify them
            var hearingSensors = FindObjectsOfType<HearingSensor>();
            foreach (var sensor in hearingSensors)
            {
                sensor.OnSoundDetected(soundPosition, gunshotSoundIntensity);
            }
        }
    }
}

