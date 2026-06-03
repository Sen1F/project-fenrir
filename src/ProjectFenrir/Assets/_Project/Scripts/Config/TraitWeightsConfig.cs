using System;
using System.Collections.Generic;

namespace Fenrir.Config
{
    /// <summary>
    /// Deserialized from StreamingAssets/Config/TraitWeights.json.
    /// Maps event keys to per-trait float deltas.
    /// </summary>
    [Serializable]
    public class TraitWeightsConfig
    {
        /// <summary>Key = BehaviorEvent.EventKey, Value = TraitKey → delta</summary>
        public Dictionary<string, Dictionary<string, float>> Weights = new()
        {
            // ── Combat ───────────────────────────────────────────────────────
            ["DodgeUsedEvent"]              = new() { ["Patience"] =  2.0f, ["Aggression"] = -1.5f },
            ["CounterLandedEvent"]          = new() { ["Dominance"] = 3.0f, ["Wisdom"]     =  2.0f },
            ["PerfectBlockEvent"]           = new() { ["Patience"]  = 3.0f, ["Dominance"]  =  2.0f },
            ["BlockUsedEvent"]              = new() { ["Patience"]  = 1.5f, ["Sacrifice"]  =  1.0f },
            ["HeavyAttackLandedEvent"]      = new() { ["Aggression"] = 1.5f },
            ["LightAttackLandedEvent"]      = new() { ["Patience"]  = 1.0f, ["Wisdom"]     =  0.5f },
            ["AbilityUsedFullEnergyEvent"]  = new() { ["Patience"]  = 2.5f, ["Wisdom"]     =  1.5f },
            ["AbilityUsedLowEnergyEvent"]   = new() { ["Recklessness"] = 2.0f },
            ["CombatCompletedNoDodgeEvent"] = new() { ["Aggression"] = 2.0f, ["Recklessness"] = 1.0f },
            ["HitTakenNoDodgeEvent"]        = new() { ["Recklessness"] = 1.5f, ["Sacrifice"] = 1.0f },

            // ── Death ────────────────────────────────────────────────────────
            ["DeathRecklessEvent"]          = new() { ["Recklessness"] = 4.0f, ["Patience"] = -2.0f },
            ["DeathSacrificeEvent"]         = new() { ["Sacrifice"]    = 3.0f, ["Recklessness"] = 1.5f },
            ["DeathOverwhelmedEvent"]       = new() { ["Recklessness"] = -1.0f, ["Wisdom"]  = -1.0f },
            ["DeathPatternFailEvent"]       = new() { ["Wisdom"]       = -3.0f },
            // DeathAmbushEvent → no trait shift (handled in TraitAccumulator)

            // ── Creature ─────────────────────────────────────────────────────
            ["CreatureSparedEvent"]         = new() { ["Mercy"]        = 3.0f, ["Curiosity"] = 1.5f },
            ["CreatureHuntedRepeatEvent"]   = new() { ["Dominance"]    = 2.0f, ["Aggression"] = 1.5f },

            // ── World ────────────────────────────────────────────────────────
            ["DialogueMercyEvent"]          = new() { ["Mercy"]        = 3.0f, ["Loyalty"]   = 1.5f },
            ["DialoguePunishmentEvent"]     = new() { ["Mercy"]        = -2.0f, ["Dominance"] = 2.5f },
            ["NpcHelpedEvent"]              = new() { ["Loyalty"]      = 3.0f, ["Wisdom"]    =  1.0f },
            ["NpcIgnoredEvent"]             = new() { ["Loyalty"]      = -2.0f },
            ["LoreObjectReadEvent"]         = new() { ["Wisdom"]       = 2.0f, ["Curiosity"] =  1.5f },
            ["SecretAreaDiscoveredEvent"]   = new() { ["Exploration"]  = 4.0f, ["Curiosity"] =  2.0f },
            ["OriginVillageReturnEvent"]    = new() { ["Loyalty"]      = 3.5f },
            ["AreaRevisitedEvent"]          = new() { ["Loyalty"]      = 1.5f, ["Curiosity"] =  1.0f },
            ["QuestCompletedEvent"]         = new() { ["Loyalty"]      = 2.5f, ["Wisdom"]    =  1.0f },

            // ── Economic ─────────────────────────────────────────────────────
            ["OffensiveUpgradePurchasedEvent"]  = new() { ["Aggression"]  = 2.0f },
            ["DefensiveUpgradePurchasedEvent"]  = new() { ["Sacrifice"]   = 2.0f, ["Patience"] = 1.5f },
            ["KnowledgeItemPurchasedEvent"]     = new() { ["Wisdom"]      = 2.5f, ["Curiosity"] = 1.5f },
            ["CurrencyHoardedEvent"]            = new() { ["Patience"]    = 2.0f, ["Wisdom"]   =  1.0f },
            ["CurrencySpentImmediatelyEvent"]   = new() { ["Recklessness"] = 1.5f },

            // ── Exploration ──────────────────────────────────────────────────
            ["ExtendedDangerZoneEvent"]     = new() { ["Aggression"]   = 1.5f, ["Recklessness"] = 1.5f },
            ["ExtendedSafeZoneEvent"]       = new() { ["Loyalty"]      = 1.5f, ["Patience"]     = 1.5f },
            ["AllSecretsFoundEvent"]        = new() { ["Exploration"]  = 4.0f, ["Wisdom"]       = 2.0f },
            ["NightExplorationEvent"]       = new() { ["Curiosity"]    = 1.5f },

            // ── Boss ─────────────────────────────────────────────────────────
            ["BossKilledEvent"]             = new() { ["Dominance"]    = 3.0f, ["Wisdom"]   =  2.0f },
        };
    }
}
