using Fenrir.Config;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Listens to player-driven events and emits BehaviorEvents to the bus.
    /// Called by PlayerCombat and PlayerController at the right moments.
    /// </summary>
    public class PlayerTraitEmitter : MonoBehaviour
    {
        private PlayerEnergy _energy;

        private void Awake() => _energy = GetComponent<PlayerEnergy>();

        // ── Combat signals ────────────────────────────────────────────────────

        public void OnDodgeUsed()
            => BehaviorEventBus.Emit(new DodgeUsedEvent());

        public void OnCounterLanded()
            => BehaviorEventBus.Emit(new CounterLandedEvent());

        public void OnHeavyAttackLanded()
            => BehaviorEventBus.Emit(new HeavyAttackLandedEvent());

        public void OnLightAttackLanded()
            => BehaviorEventBus.Emit(new LightAttackLandedEvent());

        public void OnAbilityUsed()
        {
            if (_energy != null && _energy.IsLow)
                BehaviorEventBus.Emit(new AbilityUsedLowEnergyEvent());
            else
                BehaviorEventBus.Emit(new AbilityUsedFullEnergyEvent());
        }

        public void OnCombatCompletedNoDodge()
            => BehaviorEventBus.Emit(new CombatCompletedNoDodgeEvent());

        // ── World signals ─────────────────────────────────────────────────────

        public void OnCreatureSpared()
            => BehaviorEventBus.Emit(new CreatureSparedEvent());

        public void OnCreatureHuntedRepeat()
            => BehaviorEventBus.Emit(new CreatureHuntedRepeatEvent());

        public void OnLoreObjectRead()
            => BehaviorEventBus.Emit(new LoreObjectReadEvent());

        public void OnSecretAreaDiscovered()
            => BehaviorEventBus.Emit(new SecretAreaDiscoveredEvent());

        public void OnNpcHelped()
            => BehaviorEventBus.Emit(new NpcHelpedEvent());

        public void OnNpcIgnored()
            => BehaviorEventBus.Emit(new NpcIgnoredEvent());

        public void OnOriginVillageReturn()
            => BehaviorEventBus.Emit(new OriginVillageReturnEvent());

        public void OnAreaRevisited()
            => BehaviorEventBus.Emit(new AreaRevisitedEvent());

        public void OnQuestCompleted()
            => BehaviorEventBus.Emit(new QuestCompletedEvent());

        // ── Dialogue signals ──────────────────────────────────────────────────

        public void OnDialogueMercy()
            => BehaviorEventBus.Emit(new DialogueMercyEvent());

        public void OnDialoguePunishment()
            => BehaviorEventBus.Emit(new DialoguePunishmentEvent());

        // ── Economic signals ──────────────────────────────────────────────────

        public void OnOffensiveUpgradePurchased()
            => BehaviorEventBus.Emit(new OffensiveUpgradePurchasedEvent());

        public void OnDefensiveUpgradePurchased()
            => BehaviorEventBus.Emit(new DefensiveUpgradePurchasedEvent());

        public void OnKnowledgeItemPurchased()
            => BehaviorEventBus.Emit(new KnowledgeItemPurchasedEvent());

        public void OnCurrencyHoarded()
            => BehaviorEventBus.Emit(new CurrencyHoardedEvent());

        public void OnCurrencySpentImmediately()
            => BehaviorEventBus.Emit(new CurrencySpentImmediatelyEvent());

        // ── Zone signals ──────────────────────────────────────────────────────

        public void OnExtendedDangerZone()
            => BehaviorEventBus.Emit(new ExtendedDangerZoneEvent());

        public void OnExtendedSafeZone()
            => BehaviorEventBus.Emit(new ExtendedSafeZoneEvent());

        public void OnAllSecretsFound()
            => BehaviorEventBus.Emit(new AllSecretsFoundEvent());
    }
}
