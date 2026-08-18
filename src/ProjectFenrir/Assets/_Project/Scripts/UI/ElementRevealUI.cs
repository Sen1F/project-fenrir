using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fenrir.UI
{
    /// <summary>
    /// Populates the Element Reveal panel shown at the end of Awakening.
    ///
    /// Layout contract (wire in Inspector):
    ///   _elementNameLabel — TextMeshProUGUI  large centered text ("FIRE")
    ///   _elementIcon      — Image            coloured square placeholder; swap for sprite in Phase 4
    ///   _subtitleLabel    — TextMeshProUGUI  optional flavour subtitle
    ///
    /// Usage:
    ///   AwakeningSequencer enables this panel's GameObject, which triggers
    ///   OnEnable(). The panel reads CurrentElement from SaveManager and
    ///   updates its visuals. Timing (hold duration) is owned by the sequencer,
    ///   not this class — keeping concerns separate.
    ///
    /// Extension points:
    ///   - Swap _elementIcon sprite per element in Phase 4 (assign _elementSprites array).
    ///   - Trigger a particle VFX on reveal (assign _revealVFX and call Play() in OnEnable).
    ///   - Animate the label with DOTween or Unity Animation in Phase 4.
    /// </summary>
    public class ElementRevealUI : MonoBehaviour
    {
        // ── Inspector wiring ──────────────────────────────────────────────────

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _elementNameLabel;
        [SerializeField] private TextMeshProUGUI _subtitleLabel;

        [Header("Icon")]
        [SerializeField] private Image   _elementIcon;
        [SerializeField] private Sprite[] _elementSprites; // optional Phase 4 hookup

        // ── Element display data ──────────────────────────────────────────────
        // Index order matches Element enum exactly:
        //   Fire=0, Water=1, Earth=2, Air=3,
        //   Lightning=4, Metal=5, Ice=6, Nature=7,
        //   Light=8, Darkness=9, Shadow=10,
        //   Space=11, Time=12, Life=13, Death=14
        // Element.None (15) is handled separately (returns -1 from ElementIndex).

        private static readonly string[] ElementDisplayNames =
        {
            "FIRE",      // 0
            "WATER",     // 1
            "EARTH",     // 2
            "AIR",       // 3
            "LIGHTNING", // 4
            "METAL",     // 5
            "ICE",       // 6
            "NATURE",    // 7
            "LIGHT",     // 8
            "DARKNESS",  // 9
            "SHADOW",    // 10
            "SPACE",     // 11
            "TIME",      // 12
            "LIFE",      // 13
            "DEATH",     // 14
        };

        private static readonly string[] ElementSubtitles =
        {
            "The ember stirs within you.",                      // Fire
            "The current flows through your veins.",            // Water
            "You are unmoving. Unbreakable.",                   // Earth
            "You are the wind that never settles.",             // Air
            "The storm has chosen its vessel.",                 // Lightning
            "Cold and precise. Nothing bends you.",             // Metal
            "The frost remembers every moment.",                // Ice
            "Root and branch. You grow.",                       // Nature
            "You carry a warmth that cannot be extinguished.",  // Light
            "The dark knows your name.",                        // Darkness
            "You move unseen. Unforgotten.",                    // Shadow
            "Distance is just a thought away.",                 // Space
            "Time bends to your patience.",                     // Time
            "Every breath you take is a gift you give.",        // Life
            "All things end. You simply understand it.",        // Death
        };

        // Tint colours matching each element — placeholder until real sprites exist.
        private static readonly Color[] ElementColours =
        {
            new(1.00f, 0.35f, 0.10f), // Fire
            new(0.20f, 0.55f, 1.00f), // Water
            new(0.60f, 0.45f, 0.20f), // Earth
            new(0.75f, 0.95f, 1.00f), // Air
            new(0.80f, 0.80f, 0.10f), // Lightning
            new(0.60f, 0.60f, 0.65f), // Metal
            new(0.55f, 0.85f, 1.00f), // Ice
            new(0.25f, 0.75f, 0.25f), // Nature
            new(1.00f, 0.95f, 0.65f), // Light
            new(0.30f, 0.10f, 0.45f), // Darkness
            new(0.15f, 0.15f, 0.30f), // Shadow
            new(0.65f, 0.40f, 1.00f), // Space
            new(0.90f, 0.90f, 0.60f), // Time
            new(0.90f, 0.30f, 0.50f), // Life
            new(0.20f, 0.20f, 0.20f), // Death
        };

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            // Panel becomes active → read element from save and display it.
            Element element = Element.None;
            if (ServiceLocator.TryGet<ISaveManager>(out var save))
                element = save.Current.Character.CurrentElement;

            Refresh(element);
        }

        // ── Display ───────────────────────────────────────────────────────────

        private void Refresh(Element element)
        {
            int idx = ElementIndex(element);

            // Name label
            if (_elementNameLabel != null)
            {
                _elementNameLabel.text = idx >= 0 ? ElementDisplayNames[idx] : "UNKNOWN";
                if (idx >= 0 && idx < ElementColours.Length)
                    _elementNameLabel.color = ElementColours[idx];
            }

            // Subtitle
            if (_subtitleLabel != null)
                _subtitleLabel.text = idx >= 0 ? ElementSubtitles[idx] : string.Empty;

            // Icon colour / sprite
            if (_elementIcon != null)
            {
                if (_elementSprites != null && idx >= 0 && idx < _elementSprites.Length
                    && _elementSprites[idx] != null)
                {
                    _elementIcon.sprite = _elementSprites[idx];
                    _elementIcon.color  = Color.white;
                }
                else
                {
                    // Placeholder: solid colour square
                    _elementIcon.color = idx >= 0 ? ElementColours[idx] : Color.grey;
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Maps Element enum value to the 0-based arrays above.
        /// Element.None (15) returns -1. Fire (0) → 0, Water (1) → 1, etc.
        /// </summary>
        private static int ElementIndex(Element element)
        {
            if (element == Element.None) return -1;
            int raw = (int)element;
            return raw < ElementDisplayNames.Length ? raw : -1;
        }
    }
}
