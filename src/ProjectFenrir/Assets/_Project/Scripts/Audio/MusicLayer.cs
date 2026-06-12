using System.Collections;
using UnityEngine;

namespace Fenrir.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicLayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] [Range(0f, 1f)] private float _targetVolume = 0.7f;

        private Coroutine _fadeCoroutine;

        private void Reset() => _source = GetComponent<AudioSource>();
        private void Awake()  { if (_source == null) _source = GetComponent<AudioSource>(); }

        public void CrossFade(AudioClip clip, float duration)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(DoCrossFade(clip, duration));
        }

        public void FadeOut(float duration)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(DoFade(0f, duration, stopAfter: true));
        }

        private IEnumerator DoCrossFade(AudioClip clip, float duration)
        {
            if (_source.isPlaying)
            {
                yield return DoFade(0f, duration * 0.5f, stopAfter: true);
            }

            _source.clip = clip;
            _source.loop = true;
            _source.Play();

            yield return DoFade(_targetVolume, duration * 0.5f, stopAfter: false);
        }

        private IEnumerator DoFade(float targetVolume, float duration, bool stopAfter)
        {
            float startVolume = _source.volume;
            float elapsed     = 0f;

            while (elapsed < duration)
            {
                elapsed         += Time.unscaledDeltaTime;
                _source.volume   = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            _source.volume = targetVolume;
            if (stopAfter) _source.Stop();
        }
    }
}
