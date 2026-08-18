using Fenrir.Awakening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fenrir.UI
{
    /// <summary>
    /// Drives the Character Creation panel in the Awakening scene.
    ///
    /// Layout contract (wire in Inspector):
    ///   _nameField          — TMP_InputField  (player name)
    ///   _genderToggles      — Toggle[2]       (index 0 = Male, 1 = Female)
    ///   _facePresetButtons  — Button[4]       (placeholder face images)
    ///   _bodyTypeButtons    — Button[2]       (placeholder body types)
    ///   _confirmButton      — Button
    ///   _errorLabel         — TextMeshProUGUI (hidden until validation fails)
    ///   _sequencer          — AwakeningSequencer (root of Awakening scene)
    ///
    /// Data flow:
    ///   Player fills fields → taps Confirm → OnConfirm() validates → pushes
    ///   values to AwakeningSequencer → calls ConfirmCreation().
    ///
    /// Notes:
    ///   - Gender is stored as "Male" / "Female" string in SaveData.Character.Gender.
    ///   - FacePreset / BodyType are 0-based integer indices.
    ///   - Slot index defaults to 0 (single-slot MVP).
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        // ── Inspector wiring ──────────────────────────────────────────────────

        [Header("Input controls")]
        [SerializeField] private TMP_InputField _nameField;
        [SerializeField] private Toggle[]       _genderToggles;    // [0]=Male, [1]=Female
        [SerializeField] private Button[]       _facePresetButtons;
        [SerializeField] private Button[]       _bodyTypeButtons;

        [Header("Navigation")]
        [SerializeField] private Button             _confirmButton;
        [SerializeField] private TextMeshProUGUI    _errorLabel;
        [SerializeField] private AwakeningSequencer _sequencer;

        // ── State ─────────────────────────────────────────────────────────────

        private int _selectedFacePreset = 0;
        private int _selectedBodyType   = 0;

        // Colour applied to the selected button's image to distinguish it.
        private static readonly Color SelectedTint   = new(0.55f, 0.85f, 1.0f);
        private static readonly Color DeselectedTint = Color.white;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            // Face preset buttons
            for (int i = 0; i < _facePresetButtons.Length; i++)
            {
                int captured = i;
                _facePresetButtons[i].onClick.AddListener(() => SelectFacePreset(captured));
            }

            // Body type buttons
            for (int i = 0; i < _bodyTypeButtons.Length; i++)
            {
                int captured = i;
                _bodyTypeButtons[i].onClick.AddListener(() => SelectBodyType(captured));
            }

            _confirmButton.onClick.AddListener(OnConfirm);

            // Initialise visual state
            RefreshFacePresetTints();
            RefreshBodyTypeTints();
            if (_errorLabel) _errorLabel.gameObject.SetActive(false);
        }

        // ── Selection handlers ────────────────────────────────────────────────

        private void SelectFacePreset(int index)
        {
            _selectedFacePreset = index;
            RefreshFacePresetTints();
        }

        private void SelectBodyType(int index)
        {
            _selectedBodyType = index;
            RefreshBodyTypeTints();
        }

        // ── Confirm / validation ──────────────────────────────────────────────

        private void OnConfirm()
        {
            string name = _nameField != null ? _nameField.text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                ShowError("Please enter a name.");
                return;
            }

            if (name.Length > 24)
            {
                ShowError("Name must be 24 characters or fewer.");
                return;
            }

            HideError();

            // Push data to AwakeningSequencer before confirming.
            _sequencer.SetCharacterName(name);
            _sequencer.SetCharacterGender(ResolveGender());
            _sequencer.SetCharacterFacePreset(_selectedFacePreset);
            _sequencer.SetCharacterBodyType(_selectedBodyType);
            _sequencer.SetSlotIndex(0); // single-slot MVP
            _sequencer.ConfirmCreation();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string ResolveGender()
        {
            if (_genderToggles == null || _genderToggles.Length < 2) return "Male";
            return _genderToggles[1] != null && _genderToggles[1].isOn ? "Female" : "Male";
        }

        private void RefreshFacePresetTints()
        {
            for (int i = 0; i < _facePresetButtons.Length; i++)
            {
                var img = _facePresetButtons[i].GetComponent<Image>();
                if (img) img.color = i == _selectedFacePreset ? SelectedTint : DeselectedTint;
            }
        }

        private void RefreshBodyTypeTints()
        {
            for (int i = 0; i < _bodyTypeButtons.Length; i++)
            {
                var img = _bodyTypeButtons[i].GetComponent<Image>();
                if (img) img.color = i == _selectedBodyType ? SelectedTint : DeselectedTint;
            }
        }

        private void ShowError(string message)
        {
            if (_errorLabel == null) return;
            _errorLabel.text = message;
            _errorLabel.gameObject.SetActive(true);
        }

        private void HideError()
        {
            if (_errorLabel) _errorLabel.gameObject.SetActive(false);
        }
    }
}
