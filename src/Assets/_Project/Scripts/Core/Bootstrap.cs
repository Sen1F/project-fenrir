using System.IO;
using System.Threading.Tasks;
using Fenrir.Analytics;
using Fenrir.Awakening;
using Fenrir.Config;
using Fenrir.Evolution;
using Fenrir.Save;
using Fenrir.StateMachine;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Scene index 0. Bootstraps all services, loads configs from StreamingAssets,
    /// then routes to the correct first scene via SceneRouter.
    /// Marked DontDestroyOnLoad — lives for the entire session.
    /// </summary>
    [RequireComponent(typeof(AppStateMachine))]
    [RequireComponent(typeof(GameLoop))]
    public class Bootstrap : MonoBehaviour
    {
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            RegisterServices();
            await LoadSaveData();
            await RouteToInitialSceneAsync();
        }

        private void OnEnable()
        {
            // After every full scene transition, GameLoop clears the bus.
            // Bootstrap re-subscribes the accumulator immediately so trait
            // tracking resumes in the new scene. Additive sub-zone loads
            // do NOT clear the bus, so no re-subscription needed there.
            SceneRouter.OnSceneLoadComplete += ResubscribeAccumulator;
        }

        private void OnDisable()
        {
            SceneRouter.OnSceneLoadComplete -= ResubscribeAccumulator;
        }

        private void ResubscribeAccumulator()
        {
            if (ServiceLocator.TryGet<ITraitAccumulator>(out ITraitAccumulator acc))
            {
                SubscribeAccumulatorToBus(acc);
                Debug.Log("[Bootstrap] TraitAccumulator re-subscribed after scene transition.");
            }
        }

        // ── Service registration ──────────────────────────────────────────────

        private void RegisterServices()
        {
            // Core loop
            ServiceLocator.Register<AppStateMachine>(GetComponent<AppStateMachine>());
            ServiceLocator.Register<GameLoop>(GetComponent<GameLoop>());

            // Save
            ServiceLocator.Register<ISaveManager>(new SaveManager());

            // Awakening
            ServiceLocator.Register<ElementSeedService>(new ElementSeedService());

            // Config — load JSON from StreamingAssets; fall back to C# defaults
            TraitWeightsConfig      traitWeights    = LoadConfig<TraitWeightsConfig>("TraitWeights.json")
                                                      ?? new TraitWeightsConfig();
            EvolutionSignaturesConfig evolutionConfig = LoadConfig<EvolutionSignaturesConfig>("EvolutionSignatures.json")
                                                      ?? new EvolutionSignaturesConfig();

            ServiceLocator.Register<TraitWeightsConfig>(traitWeights);

            // Evolution
            ServiceLocator.Register<IEvolutionChecker>(new EvolutionChecker(evolutionConfig));

            // Analytics
            ServiceLocator.Register<EventLogger>(new EventLogger(verbose: Debug.isDebugBuild));

            Debug.Log("[Bootstrap] Services registered.");
        }

        // ── Save loading ──────────────────────────────────────────────────────

        private async Task LoadSaveData()
        {
            ISaveManager save = ServiceLocator.Get<ISaveManager>();
            bool loaded = await save.LoadAsync();

            // Wire accumulator only when there is a save to load from.
            // New players get their accumulator created after Awakening.
            if (loaded)
                WireTraitAccumulator(save.Current.Character.Traits);

            Debug.Log($"[Bootstrap] Save loaded: {loaded}");
        }

        private void WireTraitAccumulator(Traits.TraitProfile profile)
        {
            var accumulator = new TraitAccumulator(
                profile,
                ServiceLocator.Get<TraitWeightsConfig>(),
                ServiceLocator.Get<IEvolutionChecker>()
            );
            ServiceLocator.Register<ITraitAccumulator>(accumulator);
            SubscribeAccumulatorToBus(accumulator);
        }

        /// <summary>
        /// Subscribes the accumulator to every concrete BehaviorEvent type.
        /// Called on initial load and after every full scene transition (bus clear).
        /// BehaviorEventBus.Subscribe is idempotent — safe to call multiple times.
        /// </summary>
        private static void SubscribeAccumulatorToBus(ITraitAccumulator acc)
        {
            // Cast required — Process(BehaviorEvent) is on TraitAccumulator, not the interface
            if (acc is not TraitAccumulator accumulator) return;

            BehaviorEventBus.Subscribe<DodgeUsedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CounterLandedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<PerfectBlockEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<BlockUsedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<HeavyAttackLandedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<LightAttackLandedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<AbilityUsedFullEnergyEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<AbilityUsedLowEnergyEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CombatCompletedNoDodgeEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<HitTakenNoDodgeEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DeathRecklessEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DeathSacrificeEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DeathOverwhelmedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DeathPatternFailEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DeathAmbushEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CreatureSparedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CreatureHuntedRepeatEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DialogueMercyEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DialoguePunishmentEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<NpcHelpedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<NpcIgnoredEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<LoreObjectReadEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<SecretAreaDiscoveredEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<OriginVillageReturnEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<AreaRevisitedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<QuestCompletedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<OffensiveUpgradePurchasedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<DefensiveUpgradePurchasedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<KnowledgeItemPurchasedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CurrencyHoardedEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<CurrencySpentImmediatelyEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<ExtendedDangerZoneEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<ExtendedSafeZoneEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<AllSecretsFoundEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<NightExplorationEvent>(accumulator.Process);
            BehaviorEventBus.Subscribe<BossKilledEvent>(accumulator.Process);
        }

        // ── Scene routing ─────────────────────────────────────────────────────

        private static async Task RouteToInitialSceneAsync()
        {
            ISaveManager save = ServiceLocator.Get<ISaveManager>();

            if (!save.Current.Character.HasAwakened)
            {
                await SceneRouter.LoadAwakeningAsync();
                return;
            }

            await SceneRouter.LoadGameAsync();
        }

        // ── Config loader ─────────────────────────────────────────────────────

        private static T LoadConfig<T>(string fileName) where T : class
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Config", fileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Bootstrap] Config not found at {path} — using C# defaults.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                T config = JsonUtility.FromJson<T>(json);
                Debug.Log($"[Bootstrap] Loaded config: {fileName}");
                return config;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Bootstrap] Failed to parse {fileName}: {ex.Message}");
                return null;
            }
        }
    }
}
