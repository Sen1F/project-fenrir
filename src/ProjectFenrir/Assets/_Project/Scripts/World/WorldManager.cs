using Fenrir.Core;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.World
{
    /// <summary>
    /// Scene-level singleton for the game world.
    /// Owns day/night cycle reference, manages shrine/secret tracking,
    /// and writes world-state changes to save.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private DayNightCycle _dayNight;

        public static WorldManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (_dayNight == null) _dayNight = FindAnyObjectByType<DayNightCycle>();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;

            // Restore world time from save
            _dayNight?.InitializeFromSave(save.Current.World.TimeOfDay);

            // Subscribe to phase changes to persist time-of-day
            if (_dayNight != null)
                _dayNight.OnPhaseChanged += _ => PersistTimeOfDay();
        }

        // ── Public API (called by world scripting) ────────────────────────────

        public void MarkSecretDiscovered(string secretId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;
            var secrets = save.Current.World.DiscoveredSecrets;
            if (!secrets.Contains(secretId))
            {
                secrets.Add(secretId);
                save.MarkDirty();
                BehaviorEventBus.Emit(new SecretAreaDiscoveredEvent());
            }
        }

        public void MarkQuestCompleted(string questId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;
            var quests = save.Current.World.CompletedQuests;
            if (!quests.Contains(questId))
            {
                quests.Add(questId);
                save.MarkDirty();
                BehaviorEventBus.Emit(new QuestCompletedEvent());
            }
        }

        public void MarkShrineActivated(string shrineId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;
            var shrines = save.Current.World.ActivatedShrines;
            if (!shrines.Contains(shrineId))
            {
                shrines.Add(shrineId);
                save.MarkDirty();
            }
        }

        public void OnPlayerRested()
        {
            _dayNight?.AdvanceToNextDawn();
            PersistTimeOfDay();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void PersistTimeOfDay()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var save)) return;
            save.Current.World.TimeOfDay = _dayNight != null ? _dayNight.NormalizedTime : 0f;
            save.MarkDirty();
        }
    }
}
