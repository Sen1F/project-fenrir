using UnityEngine;

namespace Fenrir.Traits
{
    // ── Base ─────────────────────────────────────────────────────────────────

    public abstract class BehaviorEvent
    {
        public string EventKey => GetType().Name;
    }

    // ── Combat ───────────────────────────────────────────────────────────────

    public class DodgeUsedEvent              : BehaviorEvent { }
    public class CounterLandedEvent          : BehaviorEvent { }
    public class PerfectBlockEvent           : BehaviorEvent { }
    public class BlockUsedEvent              : BehaviorEvent { }
    public class HeavyAttackLandedEvent      : BehaviorEvent { }
    public class LightAttackLandedEvent      : BehaviorEvent { }
    public class AbilityUsedFullEnergyEvent  : BehaviorEvent { }
    public class AbilityUsedLowEnergyEvent   : BehaviorEvent { }  // below 30%
    public class KillStreakEvent             : BehaviorEvent { public int Count; }
    public class CombatCompletedNoDodgeEvent : BehaviorEvent { }
    public class HitTakenNoDodgeEvent        : BehaviorEvent { }

    public class DeathRecklessEvent  : BehaviorEvent { }  // died without blocking/dodging
    public class DeathSacrificeEvent : BehaviorEvent { }  // died still attacking at low HP
    public class DeathOverwhelmedEvent : BehaviorEvent { } // 3+ enemies
    public class DeathPatternFailEvent : BehaviorEvent { } // 3rd+ death to same enemy type
    public class DeathAmbushEvent    : BehaviorEvent { }  // died within 5s of entering area — no shift

    // ── Creature ─────────────────────────────────────────────────────────────

    public class CreatureSparedEvent       : BehaviorEvent { }
    public class CreatureObservedEvent     : BehaviorEvent { public float DurationSeconds; }
    public class RareCreatureFoundEvent    : BehaviorEvent { }
    public class CreatureHuntedRepeatEvent : BehaviorEvent { }   // same type hunted repeatedly

    // ── World / NPC ──────────────────────────────────────────────────────────

    public class DialogueMercyEvent       : BehaviorEvent { }
    public class DialoguePunishmentEvent  : BehaviorEvent { }
    public class NpcHelpedEvent           : BehaviorEvent { }
    public class NpcIgnoredEvent          : BehaviorEvent { }
    public class LoreObjectReadEvent      : BehaviorEvent { }
    public class SecretAreaDiscoveredEvent: BehaviorEvent { }
    public class OriginVillageReturnEvent : BehaviorEvent { }
    public class AreaRevisitedEvent       : BehaviorEvent { }
    public class ShortcutPurchasedEvent   : BehaviorEvent { }
    public class QuestCompletedEvent      : BehaviorEvent { }

    // ── Economic ─────────────────────────────────────────────────────────────

    public class OffensiveUpgradePurchasedEvent  : BehaviorEvent { }
    public class DefensiveUpgradePurchasedEvent  : BehaviorEvent { }
    public class KnowledgeItemPurchasedEvent     : BehaviorEvent { }
    public class CurrencyHoardedEvent            : BehaviorEvent { }  // large unspent pool detected
    public class CurrencySpentImmediatelyEvent   : BehaviorEvent { }

    // ── Exploration / Area ───────────────────────────────────────────────────

    public class RegionFullyExploredEvent  : BehaviorEvent { public float PercentRevealed; }
    public class DirectPathTakenEvent      : BehaviorEvent { }
    public class ExtendedDangerZoneEvent   : BehaviorEvent { }
    public class ExtendedSafeZoneEvent     : BehaviorEvent { }
    public class AllSecretsFoundEvent      : BehaviorEvent { }
    public class NightExplorationEvent     : BehaviorEvent { }   // exploring during Night phase

    // ── Evolution / Shrine ───────────────────────────────────────────────────

    public class ShrineActivatedEvent     : BehaviorEvent { }
    public class EvolutionCompleteEvent   : BehaviorEvent { public string EvolutionId; }
    public class BossKilledEvent          : BehaviorEvent { public string BossId; }
}
