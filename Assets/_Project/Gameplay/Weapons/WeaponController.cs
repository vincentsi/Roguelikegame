using System;
using ProjectRoguelike.Gameplay.Player;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Weapons
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private WeaponBase startingWeapon;
        [SerializeField] private Transform weaponSocket;

        public event Action<int, int, int> OnAmmoChanged;

        private WeaponBase _currentWeapon;
        private bool _primaryHeld;

        private void Start()
        {
            if (startingWeapon != null)
            {
                Equip(startingWeapon);
            }
        }

        private void Update()
        {
            if (_currentWeapon == null)
            {
                return;
            }

            _currentWeapon.Tick(Time.deltaTime);

            if (_primaryHeld)
            {
                if (_currentWeapon.TryFire())
                {
                    cameraController?.AddRecoil(_currentWeapon.SampleRecoil());
                }
            }
        }

        public void Equip(WeaponBase weapon)
        {
            if (weapon == null)
            {
                Debug.LogError("[WeaponController] Cannot equip null weapon!");
                return;
            }

            if (weaponSocket == null)
            {
                Debug.LogError("[WeaponController] WeaponSocket is not assigned! Weapon will be parented to WeaponController transform.");
            }

            if (_currentWeapon != null)
            {
                _currentWeapon.OnAmmoChanged -= HandleAmmoChanged;
                _currentWeapon.OnFired -= HandleWeaponFired;
                Destroy(_currentWeapon.gameObject);
            }

            // Instantiate the weapon prefab before parenting
            Transform parent = weaponSocket != null ? weaponSocket : transform;
            _currentWeapon = Instantiate(weapon, parent);
            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;
            _currentWeapon.transform.localScale = Vector3.one;

            Debug.Log($"[WeaponController] Equipped {weapon.WeaponName} at position {_currentWeapon.transform.position} (local: {_currentWeapon.transform.localPosition})");
            Debug.Log($"[WeaponController] WeaponSocket position: {(weaponSocket != null ? weaponSocket.position.ToString() : "NULL")}");

            _currentWeapon.Initialize(aimCamera);
            _currentWeapon.OnAmmoChanged += HandleAmmoChanged;
            _currentWeapon.OnFired += HandleWeaponFired;

            var snapshot = _currentWeapon.GetAmmoSnapshot();
            HandleAmmoChanged(snapshot.current, _currentWeapon.MagazineSize, snapshot.reserve);
        }

        public void StartPrimaryFire() => _primaryHeld = true;
        public void StopPrimaryFire() => _primaryHeld = false;

        public void StartAltFire()
        {
            // Placeholder for alt-fire weapons (charges, etc.)
        }

        public void StopAltFire()
        {
        }

        public void ManualReload()
        {
            _currentWeapon?.ForceReload();
        }

        public void TriggerAbility()
        {
            // Hook into ability system later.
        }

        public bool TryGetAmmoSnapshot(out int current, out int mag, out int reserve)
        {
            if (_currentWeapon == null)
            {
                current = mag = reserve = 0;
                return false;
            }

            var snapshot = _currentWeapon.GetAmmoSnapshot();
            current = snapshot.current;
            reserve = snapshot.reserve;
            mag = _currentWeapon.MagazineSize;
            return true;
        }

        private void HandleAmmoChanged(int current, int mag, int reserve)
        {
            OnAmmoChanged?.Invoke(current, mag, reserve);
        }

        private void HandleWeaponFired()
        {
            // Additional hooks (VFX/audio) go here.
        }
    }
}

