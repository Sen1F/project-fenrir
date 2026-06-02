using Fenrir.Entities.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Fenrir.UI
{
    /// <summary>
    /// Root HUD controller. Owns HP bar, energy bar, and phase indicator.
    /// All child widgets are self-contained; HUD just toggles visibility.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private Image _hpFill;
        [SerializeField] private PlayerHealth _health;

        [Header("Panels")]
        [SerializeField] private GameObject _combatPanel;
        [SerializeField] private GameObject _journalNotification;

        private void Awake()
        {
            if (_health == null)
                _health = FindObjectOfType<PlayerHealth>();

            if (_health != null)
            {
                _health.OnDamaged += UpdateHpBar;
                _health.OnDied   += ShowDeathScreen;
            }
        }

        private void UpdateHpBar(float normalised)
        {
            if (_hpFill) _hpFill.fillAmount = normalised;
        }

        private void ShowDeathScreen()
        {
            // Post-MVP: full death screen. For now just hide combat panel.
            if (_combatPanel) _combatPanel.SetActive(false);
        }

        public void ShowCombatHUD(bool show)
        {
            if (_combatPanel) _combatPanel.SetActive(show);
        }

        /// <summary>Flash the journal icon when a new vague entry is added.</summary>
        public void NotifyJournalEntry()
        {
            if (_journalNotification) _journalNotification.SetActive(true);
            Invoke(nameof(HideJournalNotification), 3f);
        }

        private void HideJournalNotification()
        {
            if (_journalNotification) _journalNotification.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDamaged -= UpdateHpBar;
                _health.OnDied   -= ShowDeathScreen;
            }
        }
    }
}
