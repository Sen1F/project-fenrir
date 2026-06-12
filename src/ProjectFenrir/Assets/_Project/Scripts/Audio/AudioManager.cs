using System.Collections;
using Fenrir.Core;
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

        private bool         _inCombat;
        private DayPhase     _currentPhase = DayPhase.Day;
        private DayNightCycle _cycle;

        private void Awake()
        {
            if (ServiceLocator.TryGet<AudioManager>(out AudioManager existing) && existing != this)
            {
                Destroy(gameObject);
                return;
            }
            ServiceLocator.Register<AudioManager>(this);
            DontDestroyOnLoad(gameObject);
            if (_musicLayer == null) _musicLayer = GetComponent<MusicLayer>();
            if (_sfxPool    == null) _sfxPool    = GetComponent<SFXPool>();
        }

        private void OnEnable()  => SceneRouter.OnSceneLoadComplete += HookSceneReferences;
        private void OnDisable() => SceneRouter.OnSceneLoadComplete -= HookSceneReferences;

        private void Start() => HookSceneReferences();

        private void OnDestroy()
        {
            if (_cycle != null) _cycle.OnPhaseChanged -= OnPhaseChanged;
            BehaviorEventBus.Unsubscribe<BossKilledEvent>(OnBossKilled);
            if (ServiceLocator.TryGet<AudioManager>(out AudioManager current) && current == this)
                ServiceLocator.Unregister<AudioManager>();
        }

        /// <summary>
        /// Re-acquires scene-bound references after every full scene transition.
        /// This object survives via DontDestroyOnLoad while DayNightCycle does not,
        /// and BehaviorEventBus.Clear() wipes the boss-kill handler on transition.
        /// </summary>
        private void HookSceneReferences()
        {
            if (_cycle != null) _cycle.OnPhaseChanged -= OnPhaseChanged;
            _cycle = FindAnyObjectByType<DayNightCycle>();
            if (_cycle != null) _cycle.OnPhaseChanged += OnPhaseChanged;

            BehaviorEventBus.Subscribe<BossKilledEvent>(OnBossKilled);
        }

        private void OnBossKilled(BossKilledEvent _) => StopCombatMusic();

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
