using System.Collections;
using Fenrir.Core;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Evolution
{
    /// <summary>
    /// Orchestrates the full evolution sequence:
    /// 1. Freeze world input
    /// 2. Play shrine VFX / camera cut
    /// 3. Apply TraitProfile carryover
    /// 4. Write new evolution to save
    /// 5. Emit EvolutionCompleteEvent
    /// 6. Resume world
    ///
    /// Attach to the same GameObject as ShrineController.
    /// </summary>
    public class EvolutionSequencer : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float _buildupDuration  = 2.5f;
        [SerializeField] private float _flashDuration    = 0.4f;
        [SerializeField] private float _holdDuration     = 1.5f;

        [Header("VFX")]
        [SerializeField] private ParticleSystem _evolutionBurstFX;
        [SerializeField] private GameObject     _screenFlashOverlay;

        private bool _isRunning;

        /// <summary>Called by ShrineController when player interacts.</summary>
        public void Begin()
        {
            if (_isRunning) return;
            StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            _isRunning = true;

            // 1. Resolve best candidate
            if (!ServiceLocator.TryGet<ITraitAccumulator>(out var accumulator) ||
                !ServiceLocator.TryGet<ISaveManager>(out var save))
            {
                _isRunning = false;
                yield break;
            }

            var character = save.Current.Character;
            var candidates = accumulator.CheckEligibility(character.CurrentElement);

            if (candidates == null || candidates.Length == 0)
            {
                _isRunning = false;
                yield break;
            }

            var best = candidates[0]; // EvolutionChecker returns sorted by fit score

            // 2. Freeze player input
            SetInputEnabled(false);

            // 3. Buildup FX
            _evolutionBurstFX?.Play();
            yield return new WaitForSeconds(_buildupDuration);

            // 4. Screen flash
            if (_screenFlashOverlay != null) _screenFlashOverlay.SetActive(true);
            yield return new WaitForSeconds(_flashDuration);
            if (_screenFlashOverlay != null) _screenFlashOverlay.SetActive(false);

            // 5. Apply evolution
            accumulator.Profile.ApplyEvolutionCarryover();
            character.CurrentEvolution = best.EvolutionId;
            save.MarkDirty();
            _ = save.SaveAsync();

            // 6. Emit event
            BehaviorEventBus.Emit(new EvolutionCompleteEvent { EvolutionId = best.EvolutionId });

            // 7. Hold, then resume
            yield return new WaitForSeconds(_holdDuration);
            SetInputEnabled(true);

            _isRunning = false;
        }

        private static void SetInputEnabled(bool enabled)
        {
            if (Core.ServiceLocator.TryGet<Input.TouchMapper>(out Input.TouchMapper mapper))
                mapper.enabled = enabled;
        }
    }
}
