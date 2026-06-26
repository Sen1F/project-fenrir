#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using Fenrir.Core;
using Fenrir.Traits;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fenrir.UI
{
    /// <summary>
    /// Development-only overlay that renders all 10 trait values in real time,
    /// with colour-coding and a rolling feed of the last N trait changes.
    ///
    /// Architecture:
    ///   - Polls ITraitAccumulator.Profile each frame via ServiceLocator.
    ///   - Detects value changes by diffing against the previous frame snapshot.
    ///   - Renders via Legacy GUI (OnGUI) — zero prefab / Canvas dependencies.
    ///   - Stripped entirely from non-development builds by the #if guard.
    ///
    /// Scene setup:
    ///   Attach to any persistent GameObject in play scenes (e.g. the HUD root).
    ///   Toggle visibility: Tab key.
    ///
    /// Extension points:
    ///   - Increase MaxEventHistory to show a longer feed.
    ///   - Add a second column for session event counts (Profile.GetSessionCount).
    ///   - Replace OnGUI with an IMGUI-based EditorWindow if in-Editor UI is preferred.
    /// </summary>
    public sealed class TraitDebugHUD : MonoBehaviour
    {
        // ── Layout constants (no magic numbers) ───────────────────────────────

        private const int   PanelX          = 10;
        private const int   PanelY          = 10;
        private const int   PanelWidth      = 230;
        private const float RowHeight       = 21f;
        private const float SectionPad      = 8f;
        private const int   MaxEventHistory = 6;
        private const float PanelAlpha      = 0.78f;

        // ── Runtime state ─────────────────────────────────────────────────────

        private bool                        _visible     = true;
        private readonly Dictionary<TraitKey, float> _prev = new();
        private readonly Queue<string>      _eventFeed   = new();

        // GUIStyle fields — populated once, inside the first OnGUI call.
        private GUIStyle _boxStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _eventStyle;
        private Texture2D _panelBg;
        private bool      _stylesReady;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Update()
        {
            // Toggle visibility with Tab key (new Input System path).
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
                _visible = !_visible;

            if (!_visible) return;

            // Detect trait changes each frame and push to the rolling feed.
            if (!ServiceLocator.TryGet<ITraitAccumulator>(out ITraitAccumulator acc)) return;

            TraitProfile profile = acc.Profile;
            foreach (TraitKey key in Enum.GetValues(typeof(TraitKey)))
            {
                float current = profile.Get(key);
                if (_prev.TryGetValue(key, out float prev)
                    && MathF.Abs(current - prev) > 0.001f)
                {
                    float delta = current - prev;
                    string sign = delta >= 0f ? "+" : string.Empty;
                    _eventFeed.Enqueue($"{key}  {sign}{delta:F2}  →  {current:F1}");
                    while (_eventFeed.Count > MaxEventHistory) _eventFeed.Dequeue();
                }
                _prev[key] = current;
            }
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (!ServiceLocator.TryGet<ITraitAccumulator>(out ITraitAccumulator acc)) return;

            EnsureStyles();
            DrawPanel(acc.Profile);
        }

        private void OnDestroy()
        {
            // Prevent memory leak — Texture2D created at runtime must be destroyed manually.
            if (_panelBg != null) Destroy(_panelBg);
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void DrawPanel(TraitProfile profile)
        {
            int traitRows   = Enum.GetValues(typeof(TraitKey)).Length;
            int feedRows    = _eventFeed.Count;
            int totalRows   = traitRows + 3; // header + divider rows
            if (feedRows > 0) totalRows += feedRows + 2; // feed header + divider

            float panelHeight = totalRows * RowHeight + SectionPad * 2f;
            var   panelRect   = new Rect(PanelX, PanelY, PanelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none, _boxStyle);

            float x = PanelX + 8f;
            float y = PanelY + SectionPad;
            float w = PanelWidth - 16f;

            // ── Header ───────────────────────────────────────────────────────
            GUI.Label(new Rect(x, y, w, RowHeight), "TRAIT DEBUG  [Tab = hide]", _headerStyle);
            y += RowHeight;
            DrawDivider(x, ref y, w);

            // ── Trait rows ───────────────────────────────────────────────────
            foreach (TraitKey key in Enum.GetValues(typeof(TraitKey)))
            {
                float value = profile.Get(key);
                _rowStyle.normal.textColor = ValueColour(value);

                // Left-aligned name, right-aligned value using fixed-width string.
                string label = $"{key,-14}  {value,6:F2}";
                GUI.Label(new Rect(x, y, w, RowHeight), label, _rowStyle);
                y += RowHeight;
            }

            if (feedRows == 0) return;

            // ── Event feed ───────────────────────────────────────────────────
            DrawDivider(x, ref y, w);
            GUI.Label(new Rect(x, y, w, RowHeight), "RECENT CHANGES", _headerStyle);
            y += RowHeight;

            foreach (string line in _eventFeed)
            {
                GUI.Label(new Rect(x, y, w, RowHeight), line, _eventStyle);
                y += RowHeight;
            }
        }

        private void DrawDivider(float x, ref float y, float w)
        {
            _rowStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            GUI.Label(new Rect(x, y, w, RowHeight), "──────────────────────", _rowStyle);
            y += RowHeight * 0.7f;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Color ValueColour(float value)
        {
            if (value > 50.1f) return new Color(0.4f, 1.0f, 0.4f);   // above neutral → green
            if (value < 49.9f) return new Color(1.0f, 0.45f, 0.45f); // below neutral → red
            return Color.white;                                         // at neutral
        }

        /// <summary>
        /// Initialises GUIStyle instances. Must only be called from within OnGUI
        /// because GUI.skin is only valid during an OnGUI execution.
        /// </summary>
        private void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _panelBg = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _panelBg.SetPixel(0, 0, new Color(0f, 0f, 0f, PanelAlpha));
            _panelBg.Apply();
            _panelBg.hideFlags = HideFlags.HideAndDontSave;

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _panelBg }
            };

            _rowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Normal,
                normal    = { textColor = Color.white }
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            _eventStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Normal,
                normal    = { textColor = new Color(0.85f, 0.85f, 0.5f) }
            };
        }
    }
}
#endif
