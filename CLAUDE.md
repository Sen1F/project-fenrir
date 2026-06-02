# CLAUDE.md — Project Fenrir

Context file for Claude Code. Read this before touching any file in the repo.

---

## What this project is

**Project Fenrir** is a semi-open-world action RPG for iOS built in Unity 2022 LTS (URP).
The core mechanic is a **hidden trait system**: the player's behavior shapes 10 invisible traits which, when thresholds are met, trigger an **evolution** — a permanent, character-defining transformation.

Full design docs are in the repo root:

- `GDD.md` — master game design document
- `ARCHITECTURE.md` — Unity technical spec, folder structure, module contracts
- `HIDDEN_TRAIT_SYSTEM.md` — trait definitions, signal map, frequency dampening
- `EVOLUTION_SYSTEM.md` — tier structure, signatures, UX sequence
- `COMBAT_SYSTEM.md` — input scheme, energy, abilities, enemy archetypes, boss
- `WORLD_DESIGN.md` — Ember Forest zones, shrines, NPCs, day/night, trait signals
- `AWAKENING_SYSTEM.md` — character creation, seed architecture, reroll rules
- `LORE.md` — world history, Fenrir Principle, locked canon

Read the relevant doc before changing anything in its domain.

---

## Tech stack

| Layer | Choice |
| --- | --- |
| Engine | Unity 2022 LTS + URP |
| Language | C# (.NET Standard 2.1) |
| Rendering | URP (Universal Render Pipeline) |
| Input | Unity Input System (Enhanced Touch) |
| Physics | Unity PhysX (CharacterController for player) |
| AI | Unity NavMesh |
| Audio | Custom MusicLayer/SFXPool (FMOD post-MVP) |
| Save | JSON → `Application.persistentDataPath` |
| Keychain | Native Swift plugin via DllImport |
| Tests | Unity Test Framework (NUnit, EditMode) |

---

## Project structure

```text
src/
  Assets/
    _Project/
      Scripts/          ← all C# source, namespaced Fenrir.*
        Analytics/      ← EventLogger
        Audio/          ← AudioManager, MusicLayer, SFXPool
        Awakening/      ← AwakeningSequencer, ElementDistribution, ElementSeedService
        Combat/         ← AttackData, AttackResolver, CombatSystem, HitStateManager
        Config/         ← Element, GameConfig, TraitWeightsConfig
        Core/           ← Bootstrap, ServiceLocator, TaskExtensions
        Entities/
          Enemies/      ← EnemyBase, EnemyAI, EnemyHealth, EnemyCombat, EnemyTraitEmitter
          Player/       ← PlayerController, PlayerHealth, PlayerEnergy,
                           PlayerCombat, PlayerTraitEmitter
        Evolution/      ← EvolutionCandidate, EvolutionChecker, EvolutionSequencer,
                           EvolutionSignatureConfig, IEvolutionChecker, ShrineController
        Input/          ← InputHandler, GestureRecognizer, TouchMapper
        Save/           ← ISaveManager, SaveManager, SaveData, KeychainBridge
        StateMachine/   ← AppState, GameState, PlayerState
        Traits/         ← BehaviorEvent, BehaviorEventBus, ITraitAccumulator,
                           TraitAccumulator, TraitKey, TraitProfile
        UI/             ← EnergyBar, HUD, JournalController
        World/          ← DayNightCycle, DayPhase, RegionLoader, WorldManager
    Plugins/
      iOS/              ← KeychainPlugin.swift, iCloudPlugin.swift
    StreamingAssets/
      Config/           ← TraitWeights.json, EvolutionSignatures.json, EnemyDefinitions.json
  Tests/
    EditMode/           ← TraitAccumulatorTests, TraitProfileTests,
                           ElementDistributionTests, EvolutionCheckerTests, SaveManagerTests
```text
---

## Architecture rules

### Service Locator (not singletons)

All cross-system dependencies go through `ServiceLocator`:

```csharp
ServiceLocator.Register<ISaveManager>(new SaveManager());
ISaveManager save = ServiceLocator.Get<ISaveManager>();
ServiceLocator.TryGet<ITraitAccumulator>(out var acc); // safe, returns bool
```text
Never use `FindObjectOfType` for services. Never use static singletons for game logic.

### BehaviorEventBus

Typed event bus for trait signals. **Keyed by concrete type** — base-type subscriptions don't propagate.

```csharp
BehaviorEventBus.Subscribe<DodgeUsedEvent>(handler);
BehaviorEventBus.Unsubscribe<DodgeUsedEvent>(handler);
BehaviorEventBus.Emit(new DodgeUsedEvent());
BehaviorEventBus.Clear(); // call on scene unload
```text
All 36 concrete event types are subscribed in `Bootstrap.LoadSaveData()`.
Adding a new event requires: (1) new class in `BehaviorEvent.cs`, (2) weight entry in `TraitWeightsConfig.cs` + `TraitWeights.json`, (3) subscribe in `Bootstrap`, (4) emit site.

### Namespaces

Everything is `Fenrir.*`. Match the folder:

- `Fenrir.Config`, `Fenrir.Traits`, `Fenrir.Evolution`, `Fenrir.Save`
- `Fenrir.Awakening`, `Fenrir.Core`, `Fenrir.Combat`, `Fenrir.Input`
- `Fenrir.Entities.Player`, `Fenrir.Entities.Enemies`
- `Fenrir.World`, `Fenrir.UI`, `Fenrir.Audio`, `Fenrir.Analytics`
- `Fenrir.StateMachine`

### No magic numbers

All constants live in `GameConfig.cs`. Never hardcode floats or strings inline.

---

## Key design decisions (non-obvious, don't reverse without reading the doc)

### Traits

- 10 traits, all start at **50.0** (neutral), range 0–100.
- **Hidden** — never shown as numbers to the player. Vague journal entries only.
- Frequency dampening: after 5 identical events per session, delta halves each additional time (`Mathf.Pow(0.5f, steps+1)`).
- On evolution: **30% carryover toward neutral** (`TraitProfile.ApplyEvolutionCarryover()`). Session event counts reset.
- Trait decay is defined but **disabled in MVP** (`TraitDecayRatePerDay = 0.02f`).

### Evolution

- **Tier 1 is permanent** — once evolved, that evolution never changes.
- Checked via `IEvolutionChecker.Check(TraitProfile, Element) → EvolutionCandidate[]`.
- Multiple candidates → highest `FitScore` wins (sum of per-threshold fit contributions).
- Element must match exactly — a Fire player can only hit Fire evolutions.
- MVP has 4 evolutions: Inferno (Fire), Abyssal Current (Water), Bedrock (Earth), Galeform (Air).

### Awakening / Keychain

- Seeds live in iOS Keychain, **never iCloud**, never in save JSON.
- **One reroll per slot ever** — `RerollUsed` flag persists even if the character is deleted.
- Delete + recreate = same original seed element (pre-reroll).
- `MvpCommonOnly = true` in `ElementDistribution` — non-Common elements fall back to Common in Phase 1.

### Death

Five death types emit different trait events:

1. **Ambush** (died within 5s of combat start) → `DeathAmbushEvent` → no trait shift
2. **Pattern Failure** (3rd+ death vs same enemy) → `DeathPatternFailEvent`
3. **Reckless** (3+ unblocked hits, no dodge) → `DeathRecklessEvent`
4. **Sacrifice** (attacking at low HP — set by `PlayerCombat`) → `DeathSacrificeEvent`
5. **Overwhelmed** (fallback) → `DeathOverwhelmedEvent`

Currency loss on death: 15–25% of current currency (`GameConfig.DeathCurrencyLossMin/Max`).

### Elemental resistances

Ability hits only (non-ability hits ignore element). Same element: −25%. Opposite: +25%.
Opposites: Fire↔Water, Earth↔Air, Lightning↔Metal, Ice↔Nature, Light↔Darkness, Space↔Time, Life↔Death, Shadow→Light.
Implemented in `AttackResolver.Resolve()` + `ElementExtensions.AreOpposites()`.

### Save

- JSON via `JsonUtility` (not Newtonsoft) to `Application.persistentDataPath/save.json`.
- `SaveManager(overridePath)` constructor for test isolation — use this in EditMode tests.
- `MarkDirty()` + `SaveAsync()` pattern — never auto-save on every frame.
- `SaveData.CurrentVersion = "0.1.0"` — bump on breaking schema changes.

### Day/Night cycle

20-minute real-time cycle (`GameConfig.DayNightCycleDurationSeconds = 1200f`).
Phases: Dawn (0–15%), Day (15–50%), Dusk (50–70%), Night (70–100%).
`DayNightCycle` emits `NightExplorationEvent` when Night begins.
`WorldManager` persists `NormalizedTime` to save on each phase change.

---

## Git conventions

Branch names: `feature/name`, `fix/name`, `chore/name`, `release/version`
Commit format (enforced by hook): `type(scope): subject`
Types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`

Examples:

```text
feat(traits): add DeathSacrificeEvent classification
fix(combat): clamp energy spend to available amount
test(save): add round-trip SaveManager tests
```text
---

## Running tests

Unity EditMode tests live in `src/Tests/EditMode/`.
Run via Unity Test Runner (Window → General → Test Runner → EditMode).
No PlayMode tests yet — those come in Phase 2.

Key test files:

- `TraitAccumulatorTests` — frequency dampening, clamping, carryover
- `TraitProfileTests` — neutral factory, clamp, session counts
- `EvolutionCheckerTests` — eligibility, element gate, fit score
- `ElementDistributionTests` — determinism, diversity, no `Element.None`
- `SaveManagerTests` — round-trip, version, overridePath isolation

---

## Current phase

**Phase 1 — Repo / Foundation** (in progress)
All C# scaffold is written. Next steps:

- Unity project setup: create scenes (Bootstrap, Awakening, EmberForest), wire GameObjects to scripts
- Set player tag to `"Player"` (required by `EnemyAI.FindPlayer()` and trigger volumes)
- Configure NavMesh in EmberForest scene
- Set up Cinemachine FreeLook for third-person camera
- Assign `DayNightCycle` directional light + gradient/curve in Inspector
- Wire `InputHandler → GestureRecognizer → TouchMapper → PlayerController/PlayerCombat` in scene
- Load JSON configs from StreamingAssets at runtime (currently using C# defaults — Phase 2)
- Write native Xcode build post-process script to embed Swift plugins

See `ROADMAP.md` for full phase breakdown.
