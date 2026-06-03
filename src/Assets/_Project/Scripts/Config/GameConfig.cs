namespace Fenrir.Config
{
    /// <summary>
    /// All magic numbers live here. Nothing hardcoded elsewhere.
    /// </summary>
    public static class GameConfig
    {
        // ── Trait System ─────────────────────────────────────────────────────
        public const float TraitNeutral          = 50f;
        public const float TraitMin              = 0f;
        public const float TraitMax              = 100f;
        public const float TraitEvolutionCarryover = 0.30f; // 30% shift toward neutral on evolution
        public const float TraitDecayRatePerDay  = 0.02f;   // disabled in MVP
        public const int   FrequencyDampenAfter  = 5;       // dampening kicks in after N identical events/session

        // ── Awakening ────────────────────────────────────────────────────────
        public const int   MaxCharacterSlots     = 2;
        public const float SupremeWeight         = 0.001f;  // 0.1%
        public const float VeryRareWeight        = 0.009f;  // 0.9%
        public const float RareWeight            = 0.060f;  // 6.0%
        // Common fills remainder ~93%

        // ── Combat ───────────────────────────────────────────────────────────
        public const float EnergyMax             = 100f;
        public const float EnergyLowThreshold   = 0.25f;   // bar colour shifts below 25%
        public const float EnergyDecayDelay      = 3f;      // seconds before out-of-combat decay
        public const float EnergyDecayRate       = 5f;      // per second
        public const float CounterWindowSeconds  = 0.3f;
        public const float PerfectBlockWindow    = 0.2f;
        public const float DodgeInvincibilitySeconds = 0.2f;
        public const float DodgeCooldownSeconds  = 0.6f;
        public const float HeavyAttackChargeSeconds = 0.5f;
        public const float BaseBlockDamageReduction = 0.60f;

        // ── Day / Night ──────────────────────────────────────────────────────
        public const float DayNightCycleDurationSeconds = 1200f; // 20 minutes

        // ── XP ───────────────────────────────────────────────────────────────
        public const float XpMultiplierBossKill      = 5f;
        public const float XpMultiplierSecretArea    = 2f;
        public const float XpMultiplierQuestComplete = 3f;
        public const float XpMultiplierLoreRead      = 0.5f;
        public const float XpMultiplierRareCreature  = 1.5f;

        // ── Death classification ──────────────────────────────────────────────
        public const float DeathCurrencyLossMin      = 0.15f;
        public const float DeathCurrencyLossMax      = 0.25f;
        /// <summary>HP fraction below which the player is considered "fighting at low HP" (Sacrifice death).</summary>
        public const float SacrificeHpThreshold     = 0.25f;
        /// <summary>Max fraction of an enemy's HP the player may have dealt and still qualify as "minor damage".</summary>
        public const float SacrificeMinorDamageMax   = 0.25f;
        /// <summary>Minimum enemy count in the encounter to qualify as a group fight for Sacrifice detection.</summary>
        public const int   SacrificeGroupEnemyMin    = 2;

        // ── Elemental Resistance ─────────────────────────────────────────────
        public const float ElementalResistanceMod  = -0.25f; // same element
        public const float ElementalWeaknessMod    =  0.25f; // opposite element

        // ── Save ─────────────────────────────────────────────────────────────
        public const string SaveFileName         = "save.json";
        public const string KeychainSlotSeedKey  = "fenrir_slot_seed_{0}"; // {0} = slot index
    }
}
