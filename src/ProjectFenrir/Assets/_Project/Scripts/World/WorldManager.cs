using Fenrir.Core;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.World
{
    /// <summary>
    /// Scene-level manager for the game world.
    /// Registered with ServiceLocator on Awake — access via
    ///   ServiceLocator.Get&lt;WorldManager&gt;()
    /// No static singleton; complies with architecture rules.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private DayNightCycle _dayNight;

        private void Awake()
        {
            ServiceLocator.Register<WorldManager>(this);
            if (_dayNight == null) _dayNight = FindAnyObjectByType<DayNightCycle>();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<WorldManager>();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;

            _dayNight?.InitializeFromSave(save.Current.World.TimeOfDay);

            if (_dayNight != null)
                _dayNight.OnPhaseChanged += _ => PersistTimeOfDay();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void MarkSecretDiscovered(string secretId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
            if (save.Current.World.DiscoveredSecrets.Contains(secretId)) return;
            save.Current.World.DiscoveredSecrets.Add(secretId);
            save.MarkDirty();
            BehaviorEventBus.Emit(new SecretAreaDiscoveredEvent());
        }

        public void MarkQuestCompleted(string questId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
            if (save.Current.World.CompletedQuests.Contains(questId)) return;
            save.Current.World.CompletedQuests.Add(questId);
            save.MarkDirty();
            BehaviorEventBus.Emit(new QuestCompletedEvent());
        }

        public void MarkShrineActivated(string shrineId)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
            if (save.Current.World.ActivatedShrines.Contains(shrineId)) return;
            save.Current.World.ActivatedShrines.Add(shrineId);
            save.MarkDirty();
        }

        public void OnPlayerRested()
        {
            _dayNight?.AdvanceToNextDawn();
            PersistTimeOfDay();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void PersistTimeOfDay()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
            save.Current.World.TimeOfDay = _dayNight != null ? _dayNight.NormalizedTime : 0f;
            save.MarkDirty();
        }
    }
}
