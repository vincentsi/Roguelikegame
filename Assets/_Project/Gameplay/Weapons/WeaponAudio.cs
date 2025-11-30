using UnityEngine;

namespace ProjectRoguelike.Gameplay.Weapons
{
    /// <summary>
    /// Simple audio component for weapons. Plays sounds on fire and hit.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponAudio : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip reloadSound;

        [Header("Settings")]
        [SerializeField] private float fireVolume = 0.7f;
        [SerializeField] private float hitVolume = 0.5f;
        [SerializeField] private float reloadVolume = 0.6f;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D sound
        }

        public void PlayFireSound()
        {
            if (fireSound != null)
            {
                _audioSource.PlayOneShot(fireSound, fireVolume);
            }
        }

        public void PlayHitSound()
        {
            if (hitSound != null)
            {
                _audioSource.PlayOneShot(hitSound, hitVolume);
            }
        }

        public void PlayReloadSound()
        {
            if (reloadSound != null)
            {
                _audioSource.PlayOneShot(reloadSound, reloadVolume);
            }
        }
    }
}

