using System;
using UnityEngine;
using ProjectRoguelike.Gameplay.Weapons;

namespace ProjectRoguelike.Gameplay.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] protected string weaponName = "Prototype Rifle";
        [SerializeField] protected int magazineSize = 30;
        [SerializeField] protected int ammoReserve = 90;
        [SerializeField] protected float fireRate = 10f;
        [SerializeField] protected float reloadDuration = 1.6f;
        [SerializeField] protected Vector2 recoilKick = new Vector2(0.2f, 0.3f);
        [SerializeField] protected Transform muzzle;
        [SerializeField] protected LayerMask hitMask = ~0;

        protected Camera aimCamera;

        public int MagazineSize => magazineSize;
        public int AmmoReserve => ammoReserve;
        public string WeaponName => weaponName;

        private int _currentAmmo;
        private float _nextFireTime;
        private bool _isReloading;
        private float _reloadTimer;

        public event Action<int, int, int> OnAmmoChanged;
        public event Action OnFired;

        public virtual void Initialize(Camera camera)
        {
            aimCamera = camera;
            _currentAmmo = magazineSize;
            RaiseAmmoChanged();
        }

        public virtual void Tick(float deltaTime)
        {
            if (_isReloading)
            {
                _reloadTimer -= deltaTime;
                if (_reloadTimer <= 0f)
                {
                    CompleteReload();
                }
            }
        }

        public bool TryFire()
        {
            if (_isReloading || Time.time < _nextFireTime)
            {
                return false;
            }

            if (_currentAmmo <= 0)
            {
                return false;
            }

            _currentAmmo--;
            _nextFireTime = Time.time + 1f / fireRate;

            Vector3 origin = muzzle != null ? muzzle.position : aimCamera.transform.position;
            Vector3 direction = muzzle != null ? muzzle.forward : aimCamera.transform.forward;

            HandleShot(origin, direction);
            OnFired?.Invoke();
            RaiseAmmoChanged();
            return true;
        }

        public void ForceReload()
        {
            if (_isReloading || _currentAmmo == magazineSize || ammoReserve <= 0)
            {
                return;
            }

            _isReloading = true;
            _reloadTimer = reloadDuration;
        }

        protected virtual void CompleteReload()
        {
            _isReloading = false;
            int needed = magazineSize - _currentAmmo;
            int toLoad = Mathf.Min(needed, ammoReserve);
            ammoReserve -= toLoad;
            _currentAmmo += toLoad;
            RaiseAmmoChanged();
            
            // Play reload sound if available
            var weaponAudio = GetComponent<WeaponAudio>();
            weaponAudio?.PlayReloadSound();
        }

        public Vector2 SampleRecoil()
        {
            Vector2 recoil = new Vector2(
                UnityEngine.Random.Range(-recoilKick.x, recoilKick.x),
                UnityEngine.Random.Range(recoilKick.y * 0.5f, recoilKick.y));
            Debug.Log($"[WeaponBase] SampleRecoil: {recoil} (from recoilKick: {recoilKick})");
            return recoil;
        }

        protected void RefundShot()
        {
            _currentAmmo = Mathf.Clamp(_currentAmmo + 1, 0, magazineSize);
            RaiseAmmoChanged();
        }

        private void RaiseAmmoChanged()
        {
            OnAmmoChanged?.Invoke(_currentAmmo, magazineSize, ammoReserve);
        }

        public (int current, int reserve) GetAmmoSnapshot()
        {
            return (_currentAmmo, ammoReserve);
        }

        protected abstract void HandleShot(Vector3 origin, Vector3 direction);
    }
}

