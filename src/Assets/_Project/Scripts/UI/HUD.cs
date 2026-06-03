using Fenrir.Entities.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fenrir.UI
{
    /// <summary>
    /// Root HUD controller. Owns HP bar, energy bar, death screen, and journal notification.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private Image        _hpFill;
        [SerializeField] private PlayerHealth _health;

        [Header("Death Screen")]
        [SerializeField] private GameObject   _deathScreen;
        [SerializeField] private TMP_Text     _deathBodyText;
        [SerializeField] private TMP_Text     _deathVerdictText;

        [Header("Panels")]
        [SerializeField] private GameObject   _combatPanel;
        [SerializeField] private GameObject   _journalNotification;

        private void Awake()
        {
            if (_health == null)
                _health = FindAnyObjectByType<PlayerHealth>();

            if (_health != null)
            {
                _health.OnDamaged += UpdateHpBar;
                _health.OnDied   += ShowDeathScreen;
            }

            if (_deathScreen) _deathScreen.SetActive(false);
        }

        // ── HP ────────────────────────────────────────────────────────────────

        private void UpdateHpBar(float normalised)
        {
            if (_hpFill) _hpFill.fillAmount = normalised;
        }

        // ── Death screen ──────────────────────────────────────────────────────

        private void ShowDeathScreen()
        {
            if (_combatPanel) _combatPanel.SetActive(false);

            DeathMessageProvider.DeathMessage msg =
                DeathMessageProvider.Get(_health.LastDeathType);

            if (_deathBodyText)    _deathBodyText.text    = msg.Body;
            if (_deathVerdictText) _deathVerdictText.text = msg.Verdict;
            if (_deathScreen)      _deathScreen.SetActive(true);
        }

        public void HideDeathScreen()
        {
            if (_deathScreen) _deathScreen.SetActive(false);
        }

        // ── Combat panel ──────────────────────────────────────────────────────

        public void ShowCombatHUD(bool show)
        {
            if (_combatPanel) _combatPanel.SetActive(show);
        }

        // ── Journal notification ──────────────────────────────────────────────

        public void NotifyJournalEntry()
        {
            if (_journalNotification) _journalNotification.SetActive(true);
            Invoke(nameof(HideJournalNotification), 3f);
        }

        private void HideJournalNotification()
        {
            if (_journalNotification) _journalNotification.SetActive(false);
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            CancelInvoke(nameof(HideJournalNotification));

            if (_health != null)
            {
                _health.OnDamaged -= UpdateHpBar;
                _health.OnDied   -= ShowDeathScreen;
            }
        }
    }
}
