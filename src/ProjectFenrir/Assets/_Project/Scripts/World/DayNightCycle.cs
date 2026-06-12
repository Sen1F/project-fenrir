using System;
using Fenrir.Config;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.World
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Gradient _lightColorGradient;
        [SerializeField] private AnimationCurve _lightIntensityCurve;

        public float CycleDurationSeconds { get; private set; } = GameConfig.DayNightCycleDurationSeconds;
        public DayPhase CurrentPhase { get; private set; } = DayPhase.Dawn;
        public float NormalizedTime { get; private set; } // 0–1 over full cycle

        public event Action<DayPhase> OnPhaseChanged;

        private DayPhase _lastPhase;

        public void InitializeFromSave(float savedNormalizedTime)
        {
            NormalizedTime = savedNormalizedTime;
            _lastPhase = ComputePhase(NormalizedTime);
            CurrentPhase = _lastPhase;
            ApplyLighting();
        }

        public void AdvanceToNextDawn()
        {
            // Called when player rests at a waystone
            float dawnStart = 0f;
            NormalizedTime = dawnStart;
        }

        private void Update()
        {
            NormalizedTime = (NormalizedTime + Time.deltaTime / CycleDurationSeconds) % 1f;
            ApplyLighting();
            CheckPhaseTransition();
        }

        private void ApplyLighting()
        {
            if (_directionalLight == null) return;
            _directionalLight.color = _lightColorGradient.Evaluate(NormalizedTime);
            _directionalLight.intensity = _lightIntensityCurve.Evaluate(NormalizedTime);
            // Rotate sun: full 360° over one cycle
            _directionalLight.transform.rotation = Quaternion.Euler(NormalizedTime * 360f - 90f, 170f, 0f);
        }

        private void CheckPhaseTransition()
        {
            DayPhase newPhase = ComputePhase(NormalizedTime);
            if (newPhase == _lastPhase) return;

            _lastPhase = newPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);

            // Emit night exploration trait signal when Night begins
            if (newPhase == DayPhase.Night)
                BehaviorEventBus.Emit(new NightExplorationEvent());
        }

        private static DayPhase ComputePhase(float t) => t switch
        {
            var v when v < GameConfig.DawnEndFraction => DayPhase.Dawn,
            var v when v < GameConfig.DayEndFraction  => DayPhase.Day,
            var v when v < GameConfig.DuskEndFraction => DayPhase.Dusk,
            _                                         => DayPhase.Night
        };
    }
}
