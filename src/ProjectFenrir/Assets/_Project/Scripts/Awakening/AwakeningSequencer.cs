using System.Collections;
using Fenrir.Core;
using Fenrir.Save;
using UnityEngine;

namespace Fenrir.Awakening
{
    /// <summary>
    /// Drives the one-time awakening flow at first launch:
    /// 1. Show character creation UI
    /// 2. Derive element from Keychain seed
    /// 3. Write CharacterData to save
    /// 4. Play awakening cutscene
    /// 5. Load main game scene
    ///
    /// Attach to the Awakening scene root.
    /// </summary>
    public class AwakeningSequencer : MonoBehaviour
    {
        // Scene name removed — navigation goes through SceneRouter, never SceneManager directly

        [Header("UI Panels")]
        [SerializeField] private GameObject _characterCreationPanel;
        [SerializeField] private GameObject _elementRevealPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("Timing")]
        [SerializeField] private float _elementRevealHoldSeconds = 3f;

        // Filled by CharacterCreationUI before calling Confirm()
        private string _pendingName;
        private int    _pendingSlotIndex;

        private void Start()
        {
            ShowCharacterCreation();
        }

        // ── UI callbacks (called by CharacterCreationUI buttons) ───────────────

        public void SetCharacterName(string name)   => _pendingName      = name;
        public void SetSlotIndex(int index)         => _pendingSlotIndex = index;

        public void ConfirmCreation()
        {
            if (string.IsNullOrWhiteSpace(_pendingName)) return;
            StartCoroutine(RunAwakening());
        }

        // ── Sequence ──────────────────────────────────────────────────────────

        private IEnumerator RunAwakening()
        {
            ShowPanel(null);   // hide all

            // 1. Derive element from seed (synchronous; Keychain read is fast)
            var seedService = ServiceLocator.Get<ElementSeedService>();
            var element     = seedService.GetSlotElement(_pendingSlotIndex);

            // 2. Write character data
            if (ServiceLocator.TryGet<ISaveManager>(out var save))
            {
                var c              = save.Current.Character;
                c.Name             = _pendingName;
                c.SlotIndex        = _pendingSlotIndex;
                c.CurrentElement   = element;
                c.HasAwakened      = true;
                save.MarkDirty();
                yield return save.SaveAsync().AsCoroutine();
            }

            // 3. Reveal element
            ShowPanel(_elementRevealPanel);
            // ElementRevealUI reads save to display element name/VFX
            yield return new WaitForSeconds(_elementRevealHoldSeconds);

            // 4. Load game via SceneRouter so AppState, audio, and bus clear are all handled
            ShowPanel(_loadingPanel);
            yield return SceneRouter.LoadGameAsync().AsCoroutine();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowCharacterCreation() => ShowPanel(_characterCreationPanel);

        private void ShowPanel(GameObject panel)
        {
            if (_characterCreationPanel) _characterCreationPanel.SetActive(panel == _characterCreationPanel);
            if (_elementRevealPanel)     _elementRevealPanel    .SetActive(panel == _elementRevealPanel);
            if (_loadingPanel)           _loadingPanel          .SetActive(panel == _loadingPanel);
        }
    }
}
