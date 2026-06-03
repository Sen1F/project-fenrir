# Project Fenrir — Development Diary

A running log of what was built, why, and what's next. Written for quick re-orientation after any gap.

---

## Session 1 — Game Definition & Design Layer

**Date:** 2026-06-01

### What we did

Defined the entire game from scratch. Started with a 5-phase roadmap and worked through every major design question before writing a single line of code.

**Documents produced:**

- `GDD.md` — full game design document covering concept, pillars, systems, MVP scope
- `LORE.md` — world history, the Fenrir Principle, locked canon decisions
- `HIDDEN_TRAIT_SYSTEM.md` — 10 traits, signal map, frequency dampening, death taxonomy
- `EVOLUTION_SYSTEM.md` — tier structure, 4 MVP evolutions, fit scoring, UX sequence
- `COMBAT_SYSTEM.md` — input scheme, energy, abilities, enemy archetypes, Emberlord boss
- `WORLD_DESIGN.md` — 6 Ember Forest zones, shrines, NPCs, day/night, trait signals
- `AWAKENING_SYSTEM.md` — character creation, Keychain seed architecture, reroll rules
- `ARCHITECTURE.md` — Unity technical spec, folder structure, module contracts
- `ROADMAP.md` — phased plan with checklists
- `CHANGELOG.md`, `CONTRIBUTING.md`, `README.md`

**Key design decisions locked:**

- 10 hidden traits (Aggression, Mercy, Curiosity, Sacrifice, Dominance, Loyalty, Wisdom, Exploration, Recklessness, Patience) — all start at 50, range 0–100, never shown to player
- Trait signals come from behavior (combat style, dialogue choices, exploration, purchases) — not from explicit choices
- Frequency dampening kicks in after 5 identical events per session — prevents grinding
- Evolution Tier 1 is permanent. First evolution locks forever.
- Seeds stored in iOS Keychain — survive app deletion, never sync to iCloud
- One reroll per character slot ever — even survives character deletion
- Delete + recreate = original seed element restored
- Supreme rarity = ~0.1% total (~1 in 1,000), equivalent to pulling R9 in EAFC
- 5 death types each emit different trait signals (Ambush = no shift)
- 20-minute real-time day/night cycle, 4 phases

**Repo setup:**

- Private GitHub repo created: `Sen1F/project-fenrir`
- Git commit hook enforcing conventional commits: `type(scope): subject`

---

## Session 2 — Unity C# Scaffold (Phase 1 Foundation)

**Date:** 2026-06-02

### What we built

Wrote the entire Unity C# source tree from scratch — 56 files across 12 modules. No Unity project open yet, pure code.

**Modules written:**

| Module | Key files |
| --- | --- |
| Config | `Element.cs` (16 elements + tiers + opposites), `GameConfig.cs` (all constants), `TraitWeightsConfig.cs` (36 event→trait mappings) |
| Traits | `TraitKey.cs`, `TraitProfile.cs`, `BehaviorEvent.cs` (36 concrete events), `BehaviorEventBus.cs`, `ITraitAccumulator.cs`, `TraitAccumulator.cs` |
| Core | `Bootstrap.cs`, `ServiceLocator.cs`, `TaskExtensions.cs` |
| Evolution | `EvolutionCandidate.cs`, `EvolutionChecker.cs`, `EvolutionSequencer.cs`, `EvolutionSignatureConfig.cs`, `IEvolutionChecker.cs`, `ShrineController.cs` |
| Save | `SaveData.cs`, `ISaveManager.cs`, `SaveManager.cs`, `KeychainBridge.cs` |
| Awakening | `AwakeningSequencer.cs`, `ElementDistribution.cs`, `ElementSeedService.cs` |
| Entities/Player | `PlayerController.cs`, `PlayerHealth.cs`, `PlayerEnergy.cs`, `PlayerCombat.cs`, `PlayerTraitEmitter.cs` |
| Entities/Enemies | `EnemyBase.cs`, `EnemyAI.cs`, `EnemyHealth.cs`, `EnemyCombat.cs`, `EnemyTraitEmitter.cs` |
| Combat | `AttackData.cs`, `AttackResolver.cs`, `CombatSystem.cs`, `HitStateManager.cs` |
| Input | `InputHandler.cs`, `GestureRecognizer.cs`, `TouchMapper.cs` |
| World | `DayNightCycle.cs`, `DayPhase.cs`, `WorldManager.cs`, `RegionLoader.cs` |
| UI | `HUD.cs`, `EnergyBar.cs`, `JournalController.cs` |
| Audio | `AudioManager.cs` (expanded with day/night + combat music wiring) |
| StateMachine | `AppState.cs`, `GameState.cs`, `PlayerState.cs` |
| Analytics | `EventLogger.cs` |

**Native iOS plugins** (already existed from earlier):

- `KeychainPlugin.swift` — C-callable shims for Unity DllImport, Keychain read/write
- `iCloudPlugin.swift` — optional iCloud KV store for save sync (player opt-in only)

**JSON configs** (all complete):

- `TraitWeights.json` — 36 event→trait delta mappings
- `EvolutionSignatures.json` — 4 MVP evolution signatures with trait thresholds
- `EnemyDefinitions.json` — 5 enemies + Emberlord boss with phases

**Unit tests written:**

- `TraitAccumulatorTests.cs` — dampening, clamping, carryover
- `TraitProfileTests.cs` — neutral factory, clamp, session counts
- `EvolutionCheckerTests.cs` — eligibility, element gate, fit score
- `ElementDistributionTests.cs` — determinism, diversity, no Element.None
- `SaveManagerTests.cs` — round-trip, version, overridePath isolation

**Bugs fixed during scaffold:**

- `BehaviorEventBus` redesigned — original lambda-wrap approach made `Unsubscribe` impossible; replaced with wrapper map keyed by original handler identity
- `Bootstrap` was subscribing `BehaviorEventBus.Subscribe<BehaviorEvent>` (base type, never fires) — fixed to explicitly subscribe all 36 concrete event types
- `FindObjectOfType` deprecated in Unity 6 — replaced all with `FindAnyObjectByType`
- `[RequireComponent]` only takes 1 type per attribute — split multi-type decorators

**CLAUDE.md written** — context file for Claude Code CLI so future terminal sessions have full project context without re-explaining.

---

## Session 3 — Unity Project Setup

**Date:** 2026-06-02

### What was set up

Created the actual Unity project and wired everything up.

**Steps:**

1. Created Unity 6 LTS (6000.4.9f1) project via Unity Hub — Universal 3D template (URP)
   - Location: `/Users/seni/Documents/Claude/Projects/Project Fenrir/src/ProjectFenrir`
   - No Unity VCS — using existing Git repo
2. Updated engine target from Unity 2022 LTS → Unity 6 LTS across all docs and code
3. Added packages to `manifest.json`:
   - `com.unity.cinemachine` 3.1.4
   - `com.unity.textmeshpro` 3.2.0
   - `com.unity.burst` 1.8.18 (pinned to fix internal Burst editor compilation errors)
   - Input System, AI Navigation, Test Framework already included by template
4. Copied scripts, plugins, StreamingAssets, and Tests into Unity project
5. Wrote assembly definitions:
   - `Fenrir.Runtime.asmdef` — all game scripts
   - `Fenrir.Editor.asmdef` — editor-only scripts
   - `Fenrir.Tests.EditMode.asmdef` — EditMode tests
6. Wrote `FenrirSceneSetup.cs` editor script — runs via **Fenrir → Setup Scenes**
   - Creates Bootstrap, Awakening, EmberForest scenes
   - Adds all GameObjects with correct components
   - Adds all 3 scenes to Build Settings
7. Fixed compile errors:
   - `SaveData` lists changed from `string[]` to `List<string>` (needed `.Add()` / `.Contains()`)
   - Missing `using Fenrir.Config` in Bootstrap
   - `[RequireComponent]` multi-type syntax fixed in PlayerCombat
8. **Fenrir → Setup Scenes** run successfully — all 3 scenes created, 0 errors

**Current scene hierarchy (EmberForest):**

```text
Player          (PlayerController, PlayerHealth, PlayerEnergy, PlayerCombat, PlayerTraitEmitter, HitStateManager, CharacterController)
InputSystem     (InputHandler, GestureRecognizer, TouchMapper)
WorldManager    (WorldManager)
DayNightCycle   (DayNightCycle)
CombatSystem    (CombatSystem)
AudioManager    (AudioManager, MusicLayer, SFXPool)
HUD             (Canvas, HUD, EnergyBar, JournalController)
EventSystem
Directional Light
Main Camera
```text
---

## Current Status

**Phase 1 — Foundation:** ~90% complete

### Done ✅

- Full design layer (all docs, all decisions locked)
- Full C# scaffold (56 files, 12 modules)
- Unity project created, packages installed, scripts compiling clean
- 3 scenes created and added to Build Settings
- Unit test framework wired up

### In Progress 🔄

- Inspector references need wiring (Fenrir → Wire Scene References)

### Next Steps

1. Run **Fenrir → Wire Scene References** — assigns all serialized field references in EmberForest
2. Add player mesh / capsule placeholder so character is visible
3. Configure Cinemachine FreeLook camera on Player
4. Bake NavMesh in EmberForest
5. First playtest — player should move with keyboard (WASD), combat with J/K/L keys
6. Switch to iOS build target, test on device
7. Phase 2: load configs from StreamingAssets at runtime (currently using C# defaults)

---

*Updated: 2026-06-02*
