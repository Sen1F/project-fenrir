using Fenrir.Core;
using Fenrir.Save;
using Fenrir.Traits;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Fenrir.UI
{
    /// <summary>
    /// Manages the in-game journal panel.
    /// Entries are vague, flavour-text observations — never explicit trait values.
    /// New entries are appended on evolution and on key world events.
    /// </summary>
    public class JournalController : MonoBehaviour
    {
        [SerializeField] private GameObject _journalPanel;
        [SerializeField] private Transform  _entryContainer;
        [SerializeField] private GameObject _entryPrefab;    // prefab with a TMP_Text component
        [SerializeField] private HUD        _hud;

        private bool _isOpen;

        // ── Pre-authored vague flavour lines keyed by evolution ID ────────────
        // Keys must match EvolutionChecker signature IDs exactly (EvolutionSignatures.json)
        private static readonly Dictionary<string, string> EvolutionEntries = new()
        {
            ["inferno"]           = "The heat no longer frightens you. It feels like a memory.",
            ["abyssal_current"]   = "You move through water as though it always knew you were coming.",
            ["bedrock"]           = "Something in your chest has settled. You are harder to move.",
            ["galeform"]          = "The air parts for you now. Or perhaps it always did.",
        };

        private void Awake()
        {
            BehaviorEventBus.Subscribe<EvolutionCompleteEvent>(OnEvolution);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Toggle()
        {
            _isOpen = !_isOpen;
            _journalPanel?.SetActive(_isOpen);
            if (_isOpen) RebuildEntries();
        }

        public void AddEntry(string text)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;
            save.Current.Character.JournalEntries.Add(text);
            save.MarkDirty();
            _hud?.NotifyJournalEntry();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void OnEvolution(EvolutionCompleteEvent evt)
        {
            if (EvolutionEntries.TryGetValue(evt.EvolutionId, out string line))
                AddEntry(line);
        }

        private void RebuildEntries()
        {
            if (_entryContainer == null || _entryPrefab == null) return;

            // Clear existing
            foreach (Transform child in _entryContainer)
                Destroy(child.gameObject);

            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;

            foreach (string entry in save.Current.Character.JournalEntries)
            {
                var go   = Instantiate(_entryPrefab, _entryContainer);
                var text = go.GetComponentInChildren<TMP_Text>();
                if (text) text.text = entry;
            }
        }

        private void OnDestroy()
        {
            BehaviorEventBus.Unsubscribe<EvolutionCompleteEvent>(OnEvolution);
        }
    }
}
