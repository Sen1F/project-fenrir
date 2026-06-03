using System;
using System.Collections.Generic;
using Fenrir.Config;
using Fenrir.Traits;

namespace Fenrir.Save
{
    [Serializable]
    public class SaveData
    {
        public const string     CurrentVersion  = "0.1.0";
        public string           Version         = CurrentVersion;
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
        public TraitProfile  Traits          = TraitProfile.Neutral();
        public List<string> JournalEntries  = new();
    }

    [Serializable]
    public class WorldState
    {
        public float        TimeOfDay               = 0f;
        public string       LastCheckpointId        = "";
        public List<string> DiscoveredSecrets       = new();
        public List<string> CompletedQuests         = new();
        public List<string> ActivatedShrines        = new();
        public bool     DoctrineShrineFound     = false;
        public bool     EmberlordDefeated       = false;
    }
}
