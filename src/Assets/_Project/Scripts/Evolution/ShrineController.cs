using System;
using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Evolution;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Evolution
{
    public enum ShrineState { Dormant, Stirring, Active }

    /// <summary>
    /// World object representing an evolution shrine.
    /// Transitions through Dormant → Stirring → Active as trait thresholds are approached.
    /// When Active, interacting triggers EvolutionSequencer.
    /// </summary>
    public class ShrineController : MonoBehaviour
    {
        [SerializeField] private string        _shrineId    = "shrine_default";
        [SerializeField] private ParticleSystem _dormantFX;
        [SerializeField] private ParticleSystem _stirringFX;
        [SerializeField] private ParticleSystem _activeFX;
        [SerializeField] private EvolutionSequencer _sequencer;

        // Stirring threshold: player has ≥1 eligible evolution candidate
        // Active threshold: player has a clear best candidate (fit > 0.5)

        public ShrineState State { get; private set; } = ShrineState.Dormant;

        public event Action<ShrineState> OnStateChanged;

        private ITraitAccumulator _accumulator;
        private bool _playerInRange;

        private void Start()
        {
            ServiceLocator.TryGet<ITraitAccumulator>(out _accumulator);
            RefreshState();
        }

        // Called by save system on load
        public void InitFromSave(bool activated)
        {
            if (activated) ForceActivate();
        }

        private void Update()
        {
            // Re-evaluate shrine state each second (performance: coarse tick)
            if (Time.frameCount % 60 == 0) RefreshState();
        }

        private void RefreshState()
        {
            if (_accumulator == null) return;
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;

            Element playerElement = save.Current.Character.CurrentElement;
            var candidates = _accumulator.CheckEligibility(playerElement);

            ShrineState next;
            if (candidates.Length == 0)
                next = ShrineState.Dormant;
            else if (candidates[0].FitScore < 0.5f)
                next = ShrineState.Stirring;
            else
                next = ShrineState.Active;

            if (next != State) SetState(next);
        }

        private void SetState(ShrineState next)
        {
            State = next;
            OnStateChanged?.Invoke(next);

            _dormantFX?.gameObject.SetActive(next == ShrineState.Dormant);
            _stirringFX?.gameObject.SetActive(next == ShrineState.Stirring);
            _activeFX?.gameObject.SetActive(next == ShrineState.Active);
        }

        public void ForceActivate() => SetState(ShrineState.Active);

        // ── Interaction ───────────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = false;
        }

        /// <summary>Called by interaction system when player presses interact.</summary>
        public void TryInteract()
        {
            if (!_playerInRange || State != ShrineState.Active) return;
            _sequencer?.Begin();
        }
    }
}
