using System.Threading.Tasks;
using Fenrir.Analytics;
using Fenrir.Awakening;
using Fenrir.Core;
using Fenrir.Evolution;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fenrir.Core
{
    /// <summary>
    /// Scene index 0. Bootstraps all services and routes to the correct first scene.
    /// Marked DontDestroyOnLoad — never unloaded during a session.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string _mainMenuScene    = "MainMenu";
        [SerializeField] private string _awakeningScene   = "Awakening";
        [SerializeField] private string _gameScene        = "EmberForest";

        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            RegisterServices();
            await LoadSaveData();
            RouteToInitialScene();
        }

        private void RegisterServices()
        {
            // Save
            var saveManager = new SaveManager();
            ServiceLocator.Register<ISaveManager>(saveManager);

            // Awakening
            ServiceLocator.Register<ElementSeedService>(new ElementSeedService());

            // Load configs from StreamingAssets at runtime
            // (Full implementation in Phase 2 — stubs for now)
            var evolutionConfig  = new EvolutionSignaturesConfig();
            var traitWeights     = new TraitWeightsConfig();

            // Evolution
            var evolutionChecker = new EvolutionChecker(evolutionConfig);
            ServiceLocator.Register<IEvolutionChecker>(evolutionChecker);

            // Traits — profile comes from save; accumulator wired after load
            ServiceLocator.Register<TraitWeightsConfig>(traitWeights);

            // Analytics
            var logger = new EventLogger(verbose: Debug.isDebugBuild);
            ServiceLocator.Register<EventLogger>(logger);

            Debug.Log("[Bootstrap] Services registered.");
        }

        private async Task LoadSaveData()
        {
            ISaveManager save = ServiceLocator.Get<ISaveManager>();
            bool loaded = await save.LoadAsync();

            if (loaded)
            {
                // Wire TraitAccumulator to the loaded profile
                var accumulator = new TraitAccumulator(
                    save.Current.Character.Traits,
                    ServiceLocator.Get<TraitWeightsConfig>(),
                    ServiceLocator.Get<IEvolutionChecker>()
                );
                ServiceLocator.Register<ITraitAccumulator>(accumulator);

                // Subscribe accumulator to event bus
                BehaviorEventBus.Subscribe<BehaviorEvent>(accumulator.Process);
            }

            Debug.Log($"[Bootstrap] Save loaded: {loaded}");
        }

        private void RouteToInitialScene()
        {
            ISaveManager save = ServiceLocator.Get<ISaveManager>();

            if (!save.Current.Character.HasAwakened)
            {
                SceneManager.LoadScene(_awakeningScene);
                return;
            }

            // Resume at last known game state
            SceneManager.LoadScene(_gameScene);
        }
    }
}
