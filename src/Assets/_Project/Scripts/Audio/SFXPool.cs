using System.Collections.Generic;
using UnityEngine;

namespace Fenrir.Audio
{
    public class SFXPool : MonoBehaviour
    {
        [SerializeField] private int _poolSize = 16;
        [SerializeField] private AudioSource _prefab;

        private readonly Queue<AudioSource> _pool = new();

        private void Awake()
        {
            // If no prefab assigned, create a minimal AudioSource as the pool template
            if (_prefab == null)
            {
                var go = new GameObject("SFX_Template");
                go.transform.SetParent(transform);
                _prefab = go.AddComponent<AudioSource>();
                _prefab.playOnAwake = false;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                AudioSource source = Instantiate(_prefab, transform);
                source.playOnAwake = false;
                source.gameObject.SetActive(false);
                _pool.Enqueue(source);
            }
        }

        public void Play(AudioClip clip, Vector3 worldPosition, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetFree();
            source.transform.position = worldPosition;
            source.volume             = volume;
            source.clip               = clip;
            source.gameObject.SetActive(true);
            source.Play();

            StartCoroutine(ReturnAfterPlay(source, clip.length));
        }

        private AudioSource GetFree()
        {
            // Steal the oldest source if all are busy
            AudioSource candidate = _pool.Dequeue();
            if (candidate.isPlaying) candidate.Stop();
            _pool.Enqueue(candidate);
            return candidate;
        }

        private System.Collections.IEnumerator ReturnAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.gameObject.SetActive(false);
        }
    }
}
