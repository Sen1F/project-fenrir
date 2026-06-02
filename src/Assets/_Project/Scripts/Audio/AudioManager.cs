using System.Collections;
using UnityEngine;

namespace Fenrir.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private MusicLayer _musicLayer;
        [SerializeField] private SFXPool    _sfxPool;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
        {
            _musicLayer.CrossFade(clip, fadeDuration);
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            _musicLayer.FadeOut(fadeDuration);
        }

        public void PlaySFX(AudioClip clip, Vector3 worldPosition = default, float volume = 1f)
        {
            _sfxPool.Play(clip, worldPosition, volume);
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }
    }
}
