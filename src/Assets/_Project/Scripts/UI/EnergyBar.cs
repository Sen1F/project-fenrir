using Fenrir.Config;
using Fenrir.Entities.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Fenrir.UI
{
    /// <summary>
    /// Drives the energy bar fill and colour. No numbers shown — colour shift only.
    /// Thresholds defined in GameConfig.EnergyLowThreshold.
    /// </summary>
    public class EnergyBar : MonoBehaviour
    {
        [SerializeField] private Image        _fillImage;
        [SerializeField] private PlayerEnergy _energy;

        [Header("Colours")]
        [SerializeField] private Color _fullColor = new Color(0.95f, 0.75f, 0.20f);   // amber
        [SerializeField] private Color _midColor  = new Color(0.90f, 0.45f, 0.10f);   // orange
        [SerializeField] private Color _lowColor  = new Color(0.85f, 0.15f, 0.10f);   // red

        private void Awake()
        {
            if (_energy == null)
                _energy = FindObjectOfType<PlayerEnergy>();

            if (_energy != null)
                _energy.OnEnergyChanged += Refresh;
        }

        private void Start() => Refresh(_energy != null ? _energy.Current : GameConfig.EnergyMax);

        private void Refresh(float current)
        {
            float t = current / GameConfig.EnergyMax;
            if (_fillImage == null) return;

            _fillImage.fillAmount = t;
            _fillImage.color      = t <= GameConfig.EnergyLowThreshold ? _lowColor
                                  : t <= 0.55f                          ? _midColor
                                  : _fullColor;
        }

        private void OnDestroy()
        {
            if (_energy != null) _energy.OnEnergyChanged -= Refresh;
        }
    }
}
