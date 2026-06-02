# Project Fenrir — Architecture v0.1

> **Status:** Draft  
> **Last Updated:** 2026-06-01  
> **Phase:** 1 — Foundation

---

## Stack

| Layer | Choice | Rationale |
| --- | --- | --- |
| Engine | Unity 2022 LTS | Proven stable, long-term support, strong iOS export |
| Render pipeline | URP (Universal Render Pipeline) | Mobile-optimised, supports custom shaders, good lighting |
| Language | C# (.NET Standard 2.1) | Unity standard |
| Input | Unity New Input System | Touch-native, action-map based |
| Physics | Unity PhysX (3D) | Built-in, sufficient for melee action RPG |
| Audio | Unity Audio (MVP) → FMOD (post-MVP) | Unity Audio fast to ship; FMOD for dynamic music later |
| Save (game data) | JSON → local file (Application.persistentDataPath) | Simple, debuggable |
| Save (slot seeds) | Native iOS Keychain via Unity plugin | Tamper-resistant, survives app deletion |
| Cloud backup | iCloud Key-Value Store via Unity plugin | Optional sync for save data |
| Analytics/logging | Custom event bus (no third-party in MVP) | Full control over trait event data |
| Backend | None for MVP | Deferred until PvP/global discovery |

---

## Unity Project Structure

```text
Assets/
├── _Project/                    ← All first-party game code lives here
│   ├── Scripts/
│   │   ├── Core/                ← Bootstrap, ServiceLocator, GameLoop
│   │   ├── StateMachine/        ← AppState, GameState, PlayerState
│   │   ├── Combat/              ← CombatSystem, AttackResolver, HitStateManager
│   │   ├── Input/               ← InputHandler, GestureRecognizer, TouchMapper
│   │   ├── Entities/
│   │   │   ├── Player/          ← PlayerController, PlayerStats, PlayerAnimator
│   │   │   ├── Enemies/         ← EnemyBase, EnemyAI, archetypes/
│   │   │   └── Creatures/       ← CreatureBase, CreatureBehaviour
│   │   ├── Traits/              ← TraitProfile, TraitAccumulator, BehaviorEvent
│   │   ├── Evolution/           ← EvolutionChecker, ShrineController, EvolutionSequencer
│   │   ├── World/               ← WorldManager, RegionLoader, DayNightCycle
│   │   ├── Awakening/           ← AwakeningSequencer, ElementSeedService
│   │   ├── Save/                ← SaveManager, SaveData, KeychainBridge
│   │   ├── Audio/               ← AudioManager, MusicLayer, SFXPool
│   │   ├── UI/                  ← HUD, JournalController, MenuStack, EnergyBar
│   │   ├── Analytics/           ← EventLogger, BehaviorEventBus
│   │   └── Config/              ← GameConfig, Constants, ElementDefinitions
│   ├── Scenes/
│   │   ├── Bootstrap.unity      ← First scene, loads all services, never unloaded
│   │   ├── MainMenu.unity
│   │   ├── Awakening.unity      ← Ceremony scene
│   │   └── EmberForest.unity    ← MVP gameplay scene
│   ├── Prefabs/
│   │   ├── Entities/
│   │   ├── UI/
│   │   ├── VFX/
│   │   └── World/
│   ├── Animations/
│   ├── Materials/
│   ├── Shaders/                 ← Custom URP shaders for elemental effects
│   └── Audio/
├── Plugins/
│   └── iOS/                     ← Native Swift bridge files
│       ├── KeychainPlugin.swift
│       └── iCloudPlugin.swift
├── StreamingAssets/
│   └── Config/                  ← Runtime-editable JSON (no recompile to tune)
│       ├── TraitWeights.json
│       ├── EvolutionSignatures.json
│       └── EnemyDefinitions.json
└── Packages/                    ← Unity Package Manager
```text
`_Project/` prefix sorts first-party code to the top of the Project window and cleanly separates it from Unity packages and third-party assets. Nothing outside `_Project/` contains game logic.

---

## Architecture Pattern

### Service Locator

Each major system is a **service** registered at startup in `Bootstrap`. Other systems access services through `ServiceLocator` — never via static singletons or `FindObjectOfType`.

```csharp
// Registration (Bootstrap.cs)
ServiceLocator.Register<ITraitAccumulator>(new TraitAccumulator());
ServiceLocator.Register<ISaveManager>(new SaveManager());

// Access anywhere
var traits = ServiceLocator.Get<ITraitAccumulator>();
```text
Singletons are untestable and create hidden dependencies. Service locator with interfaces allows mocking in unit tests.

### Event Bus (Behavior Events)

Game systems communicate through a typed event bus. Game actions translate into trait signals without creating direct module dependencies.

```csharp
// CombatSystem emits
BehaviorEventBus.Emit(new DodgeUsedEvent());

// TraitAccumulator receives and updates — no direct coupling
```text
The event bus is the spine of the trait system. Every behavioral signal passes through it.

### Component-Based Entities

No inheritance chains deeper than one level from a base class.

```text
EnemyBase (MonoBehaviour)
  ├── EnemyHealth
  ├── EnemyAI          (state machine: Patrol/Chase/Attack/Stunned)
  ├── EnemyCombat
  └── EnemyTraitEmitter  ← emits BehaviorEvents on player interaction

Player (MonoBehaviour)
  ├── PlayerHealth
  ├── PlayerController  (movement + camera)
  ├── PlayerCombat      (input → attack resolution)
  ├── PlayerEnergy      (energy bar state)
  └── PlayerTraitEmitter
```text
---

## Core Module Specs

### Bootstrap / GameLoop

`Bootstrap.unity` is scene index 0. Initialises all services, reads save state, routes to correct scene. Marked `DontDestroyOnLoad` — never unloaded.

```csharp
public class Bootstrap : MonoBehaviour
{
    async void Start()
    {
        RegisterServices();
        await LoadSaveData();
        SceneRouter.NavigateToInitialScene();
    }
}
```text
### State Machine

Three levels:

| Level | States |
| --- | --- |
| AppState | MainMenu / Loading / InGame / Paused |
| GameState | Exploration / Combat / Awakening / Evolution / Cutscene |
| PlayerState | Idle / Moving / Attacking / Dodging / Blocking / Stunned |

Enum-driven, no third-party FSM library for MVP. Transitions emit events that other systems listen to.

### TraitAccumulator

```csharp
public interface ITraitAccumulator
{
    void Process(BehaviorEvent evt);
    void ApplyDecay(float daysSinceLastPlay);
    EvolutionCandidate[] CheckEligibility(Element element);
}
```text
`TraitProfile` is owned by the save system. Accumulator reads and writes through it — owns no persistent state itself.

### SaveManager

Two storage layers:

```text
Layer 1: JSON → Application.persistentDataPath/save.json
  - Character data, trait profile, world state, quest state, inventory
  - Written on every checkpoint and on app background

Layer 2: iOS Keychain (via KeychainBridge)
  - Slot seeds only (slot_seed_0, slot_seed_1)
  - Written once at account creation, never again
```text
### ElementSeedService

```csharp
public class ElementSeedService
{
    public Element GetSlotElement(int slotIndex)
    {
        var seed = KeychainBridge.GetOrCreateSeed(slotIndex);
        return ElementDistribution.Derive(seed);
    }
}
```text
### DayNightCycle

```csharp
public class DayNightCycle : MonoBehaviour
{
    public float CycleDurationSeconds = 1200f; // 20 minutes real-time
    public DayPhase CurrentPhase { get; private set; }
    public event Action<DayPhase> OnPhaseChanged;
}

public enum DayPhase { Dawn, Day, Dusk, Night }
```text
Drives: directional light, creature spawning (Inferno Wisps at Night), ambient audio crossfades, trait signal weighting.

---

## Native iOS Plugins

### KeychainPlugin.swift

```swift
@objc public class KeychainPlugin: NSObject {
    @objc public static func getSeed(key: String) -> String?
    @objc public static func setSeed(key: String, value: String) -> Bool
}
```text
C# bridge uses `[DllImport("__Internal")]` on `UNITY_IOS`. In Editor, falls back to `PlayerPrefs` (dev only — clearly marked).

### iCloudPlugin.swift

Wraps `NSUbiquitousKeyValueStore`. Player opt-in only. Syncs `save.json` content — never slot seeds (Keychain is intentionally device-bound).

---

## Config Files (StreamingAssets/Config)

All tunable values live outside the build. Threshold adjustments require no recompile.

### TraitWeights.json (sample)

```json
{
  "dodgeUsed":            { "patience": 2.0, "aggression": -1.5 },
  "counterLanded":        { "dominance": 3.0, "wisdom": 2.0 },
  "death_reckless":       { "recklessness": 4.0, "patience": -2.0 },
  "death_sacrifice":      { "sacrifice": 3.0, "recklessness": 1.5 },
  "creatureSpared":       { "mercy": 3.0, "curiosity": 1.5 },
  "secretAreaDiscovered": { "exploration": 4.0, "curiosity": 2.0 }
}
```text
### EvolutionSignatures.json (sample)

```json
{
  "inferno": {
    "element": "fire",
    "thresholds": {
      "aggression":   { "min": 70 },
      "dominance":    { "min": 65 },
      "mercy":        { "max": 35 },
      "recklessness": { "min": 55 }
    }
  }
}
```text
---

## Testing Strategy

### Unit Tests (EditMode — no Unity runtime needed)

- `TraitAccumulator` — event sequence → assert trait deltas
- `ElementSeedService` — seed → assert correct element
- `EvolutionChecker` — trait profile → assert correct candidate
- `SaveManager` — serialise/deserialise round-trip

### Integration Tests (PlayMode — runs in Unity)

- Player takes damage → event emitted → accumulator updated
- Shrine interaction + eligible profile → evolution triggered
- Save → quit → load → trait profile intact

### Keychain

Tested on-device only. Unit tests use `IKeychainBridge` mock returning a fixed seed.

---

## C# Conventions

| Rule | Detail |
| --- | --- |
| Naming | `PascalCase` classes/methods, `_camelCase` private fields, `I` prefix interfaces |
| No `var` | Only where type is obvious from the right side |
| `readonly` | All injected dependencies |
| Async | `async/await` for scene loading and IO — never block main thread |
| No `FindObjectOfType` | Use `ServiceLocator` in production code |
| No magic numbers | All constants in `GameConfig.cs` or JSON config |
| One class per file | No exceptions |

---

## Phase 1 Deliverables

- [ ] Unity 2022 LTS project created, URP configured
- [ ] `_Project/` folder structure scaffolded
- [ ] New Input System package installed and configured
- [ ] `Bootstrap.unity` scene with `ServiceLocator` skeleton
- [ ] `KeychainBridge.cs` + `KeychainPlugin.swift` stub
- [ ] `iCloudPlugin.swift` stub
- [ ] `TraitProfile.cs` + `BehaviorEvent.cs` data models
- [ ] `BehaviorEventBus.cs` implementation
- [ ] `TraitWeights.json` + `EvolutionSignatures.json` placeholders
- [ ] EditMode unit test project configured
- [ ] iOS build target: bundle ID `com.fenrir.projectfenrir`, signing placeholder
- [ ] `.gitignore` updated for Unity
- [ ] `ROADMAP.md` updated: Phase 0 complete, Phase 1 in progress
