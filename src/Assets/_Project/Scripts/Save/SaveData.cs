using System;
using Fenrir.Config;
using Fenrir.Traits;

namespace Fenrir.Save
{
    [Serializable]
    public class SaveData
    {
        public string           Version         = "0.1.0";
        public DateTime         SavedAt         = DateTime.UtcNow;
        public CharacterData    Character       = new();
        public WorldState       World           = new();
    }

    [Serializable]
    public class CharacterData
    {
        public string       Name            = "";
        public string       Gender          = "";
        public int          FacePreset      = 0;
        public int          BodyType        = 0;
        public int          SlotIndex       = 0;
        public Element      CurrentElement  = Element.None;
        public string       CurrentEvolution = "";      // "" = base form
        public int          Level           = 1;
        public float        XP              = 0f;
        public float        Currency        = 0f;
        public bool         RerollUsed      = false;
        public bool         HasAwakened     = false;
        public TraitProfile Traits          = TraitProfile.Neutral();
        public string[]     JournalEntries  = Array.Empty<string>();
    }

    [Serializable]
    public class WorldState
    {
        public float    TimeOfDay               = 0f;   // 0–1, fraction of DayNightCycle
        public string   LastCheckpointId        = "";
        public string[] DiscoveredSecrets       = Array.Empty<string>();
        public string[] CompletedQuests         = Array.Empty<string>();
        public string[] ActivatedShrines        = Array.Empty<string>();
        public bool     DoctrineShrineFound     = false;
        public bool     EmberlordDefeated       = false;
    }
}
