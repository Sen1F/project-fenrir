using System.Collections;
using Fenrir.Traits;
using Fenrir.World;
using UnityEngine;

namespace Fenrir.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private MusicLayer _musicLayer;
        [SerializeField] private SFXPool    _sfxPool;

        [Header("Phase Clips")]
        [SerializeField] private AudioClip _dawnMusic;
        [SerializeField] private AudioClip _dayMusic;
        [SerializeField] private AudioClip _duskMusic;
        [SerializeField] private AudioClip _nightMusic;
        [SerializeField] private AudioClip _combatMusic;

        private bool      _inCombat;
        private DayPhase  _currentPhase = DayPhase.Day;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (_musicLayer == null) _musicLayer = GetComponent<MusicLayer>();
            if (_sfxPool    == null) _sfxPool    = GetComponent<SFXPool>();
        }

        private void Start()
        {
            var cycle = FindAnyObjectByType<DayNightCycle>();
            if (cycle != null) cycle.OnPhaseChanged += OnPhaseChanged;

            BehaviorEventBus.Subscribe<BossKilledEvent>(_ => StopCombatMusic());
        }

        // ── Day/Night ─────────────────────────────────────────────────────────

        private void OnPhaseChanged(DayPhase phase)
        {
            _currentPhase = phase;
            if (!_inCombat) PlayPhaseMusic(phase);
        }

        private void PlayPhaseMusic(DayPhase phase)
        {
            AudioClip clip = phase switch
            {
                DayPhase.Dawn  => _dawnMusic,
                DayPhase.Day   => _dayMusic,
                DayPhase.Dusk  => _duskMusic,
                DayPhase.Night => _nightMusic,
                _              => _dayMusic
            };
            if (clip) PlayMusic(clip);
        }

        // ── Combat ────────────────────────────────────────────────────────────

        public void StartCombatMusic()
        {
            _inCombat = true;
            if (_combatMusic) PlayMusic(_combatMusic, fadeDuration: 0.5f);
        }

        public void StopCombatMusic()
        {
            _inCombat = false;
            PlayPhaseMusic(_currentPhase);
        }

        // ── Core ──────────────────────────────────────────────────────────────

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
