# Project Fenrir — Engineering Plan

**Last updated:** 2026-06-11  
**Current branch:** `feature/phase3-awakening`  
**Current phase:** Phase 2 architecture merged (PR #3/#4); P2 verification tickets open; Phase 3 starting

---

## How to read this document

Each ticket has:

- **Goal** — what done looks like
- **Steps** — exact actions in order
- **Acceptance criteria** — measurable pass/fail checks
- **Branch** — Git branch for the work

Tickets within a phase are ordered by dependency. Do not skip forward.

---

## Source tree clarification

Two `Assets/` trees exist in `src/`:

| Path | Status |
| --- | --- |
| `src/ProjectFenrir/Assets/` | ✅ **Canonical** — actual Unity project |
| `src/Assets/` | ⚠️ Legacy duplicate — predates Unity project setup |

All Unity work happens in `src/ProjectFenrir/`. The `src/Assets/` tree should be deleted once linter rules are updated to point at the correct path. **P2-01** covers this.

---

## Phase 2 — Architecture Completion

**Branch:** `chore/repo-setup` (continue until P2 complete, then merge to `main`)

---

### P2-01 · Resolve dual source tree

**Goal:** One canonical source location. Unity project compiles clean. Linter targets correct path.

**Steps:**

1. Copy `SceneValidator.cs` from `src/ProjectFenrir/Assets/_Project/Scripts/Core/` to `src/Assets/_Project/Scripts/Core/` (keeps `src/Assets/` compilable in isolation if needed).
2. Diff every file in `src/Assets/_Project/Scripts/` against its counterpart in `src/ProjectFenrir/Assets/_Project/Scripts/`. For any file where the `src/Assets/` version is newer (linter-modified), copy it into `src/ProjectFenrir/`. Use:

   ```bash
   rsync -av --checksum \
     "src/Assets/_Project/Scripts/" \
     "src/ProjectFenrir/Assets/_Project/Scripts/"
   ```

3. Delete `src/Assets/` entirely.
4. Update `.gitignore` — confirm no rule references `src/Assets/`.
5. Update `CLAUDE.md` project structure to show `src/ProjectFenrir/Assets/_Project/Scripts/` as the source root.
6. Open Unity Editor. Confirm 0 compile errors.

**Acceptance criteria:**

- `src/Assets/` no longer exists in the repo
- Unity Editor opens with 0 compile errors
- `git diff HEAD --stat` shows only the sync'd changes

**Branch:** `chore/repo-setup`

---

### P2-02 · Run all EditMode unit tests

**Goal:** All 27 EditMode tests pass green before any Phase 2 runtime work.

**Steps:**

1. Open Unity Editor.
2. Window → General → Test Runner → EditMode tab.
3. Click **Run All**.
4. If any test fails, fix before proceeding to P2-03.

**Known test suites (27 total across 5 files):**

| Suite | File | Tests |
| --- | --- | --- |
| TraitAccumulatorTests | `src/ProjectFenrir/Tests/EditMode/` | dampening, clamp, carryover |
| TraitProfileTests | same | neutral factory, clamp, session counts |
| EvolutionCheckerTests | same | eligibility, element gate, fit score |
| ElementDistributionTests | same | determinism, diversity, no Element.None |
| SaveManagerTests | same | round-trip, version, overridePath isolation |

**Acceptance criteria:**

- All 27 tests green
- Zero compilation warnings in test assembly

**Branch:** `chore/repo-setup`

---

### P2-03 · TraitDebugHUD — trait system smoke test

**Goal:** Live trait values visible on-screen in Play mode. Prove the full pipeline: keypress → `BehaviorEvent` → `BehaviorEventBus` → `TraitAccumulator` → `TraitProfile` updates.

**Design note:**  
`TraitDebugHUD` is a dev-only overlay. It must be guarded by `#if UNITY_EDITOR || DEVELOPMENT_BUILD` so it never ships to App Store builds. It reads `TraitAccumulator.Profile` each frame and renders all 10 trait values. Strip from release builds via scripting define.

**Steps:**

1. Create `src/ProjectFenrir/Assets/_Project/Scripts/UI/TraitDebugHUD.cs`:

   ```text
   Namespace:  Fenrir.UI
   Condition:  [Conditional("DEVELOPMENT_BUILD")] on the class or #if guard
   Polls:      ServiceLocator.TryGet<ITraitAccumulator>(out acc) each frame
   Renders:    OnGUI() — 10 rows, "TraitKey: value" (e.g. "Patience: 52.40")
   Position:   Top-left, 10px padding, dark semi-transparent background box
   Color:      Green if > 50, red if < 50, white if == 50
   ```

2. Add a `TraitDebugHUD` component to the `HUD` GameObject in EmberForest scene (Inspector or via editor script).

3. Open EmberForest scene. Enter Play mode.

4. Confirm `TraitDebugHUD` shows all 10 traits at 50.0 (neutral start).

5. Press **Space** (the dodge key as wired in `InputHandler`).  
   Expected: `DodgeUsedEvent` fires via `PlayerTraitEmitter` → `BehaviorEventBus` → `TraitAccumulator.Process()` → `Patience` increases by the weighted delta.

6. Open Console. Confirm `[EventLogger]` log line: `DodgeUsedEvent → Patience +X.XX | now: XX.XX`.

7. Press Space 5+ more times rapidly.  
   Expected: dampening kicks in — delta halves each time after the 5th (see `GameConfig.FrequencyDampenAfter`).

**Acceptance criteria:**

- All 10 traits render at 50.0 on first Play
- Space key → `Patience` value increases in HUD
- EventLogger log appears in Console
- 6th+ dodge shows smaller delta than 5th (dampening confirmed)
- No NullReferenceException in Console

**Branch:** `feature/trait-debug-hud`

---

### P2-04 · First enemy alive — Ash Wolf

**Goal:** One Ash Wolf capsule patrols EmberForest, detects the player, chases, and takes damage. Proves EnemyAI + CombatSystem + trait event pipeline end-to-end.

**Design note:**  
Do not spawn the enemy from a Prefab yet (prefabs come in Phase 3). For now, manually place a capsule GameObject in the EmberForest scene with all required components attached.

**Steps:**

1. **Bake NavMesh** in EmberForest:
   - Select the ground plane. In Inspector → Static dropdown → check **Navigation Static**.
   - Window → AI → Navigation → Bake tab → click **Bake**.
   - Confirm a blue NavMesh surface appears on the ground.
   - Save scene.

2. **Place Ash Wolf** in EmberForest:
   - Create a Capsule GameObject named `AshWolf`.
   - Add components: `EnemyBase`, `EnemyHealth`, `EnemyAI`, `EnemyCombat`, `EnemyTraitEmitter`, `NavMeshAgent`.
   - Set `EnemyBase` fields: `EnemyId = "ash_wolf"`, `Archetype = Aggressive`, `Element = Fire`.
   - Set `EnemyHealth` fields: `MaxHealth = 80`, starting HP = 80.
   - Set `EnemyAI` fields: `DetectionRange = 8`, `AttackRange = 1.5`, `PatrolRadius = 5`, `AttackCooldown = 1.5`.
   - Set NavMeshAgent: Speed = 3.5, Stopping Distance = 1.5.
   - Position the wolf ~10 units from the player start.

3. **Verify patrol**: Enter Play mode. Watch wolf walk a patrol path.

4. **Verify chase**: Walk player within 8 units. Wolf should turn and chase.

5. **Verify attack**: Press **J** (light attack) when adjacent to wolf.  
   Expected:
   - `PlayerCombat.Attack()` fires
   - `CombatSystem.ResolveHit()` called
   - `EnemyHealth.TakeDamage()` reduces wolf HP
   - `LightAttackLandedEvent` fires → `Aggression` trait shifts
   - Console: `[EventLogger] LightAttackLandedEvent → ...`

6. **Verify death**: Kill the wolf (attack until HP = 0).  
   Expected: Wolf GameObject is destroyed (or plays death anim placeholder). No NullRefs after death.

**Acceptance criteria:**

- NavMesh bake covers entire ground plane with no holes
- Wolf patrols without NavMesh errors
- Wolf chases player on detection
- J-key reduces wolf HP in Console logs
- `LightAttackLandedEvent` fires and `Aggression` trait moves in TraitDebugHUD
- Wolf dies cleanly (no exceptions)

**Branch:** `feature/first-enemy`

---

### P2-05 · Save/load round-trip

**Goal:** Game state survives a full Play → Stop → Play cycle. Position, traits, time-of-day all restored.

**Design note:**  
`SaveManager` writes to `Application.persistentDataPath/save.json`. In Editor this is a writable path. `MarkDirty()` + `SaveAsync()` means saves only trigger when state changes. `GameLoop` calls `SaveAsync()` on `OnApplicationPause(true)` and `OnApplicationQuit`.

**Steps:**

1. Open EmberForest scene. Enter Play mode.

2. Move the player to a position that is clearly not the spawn point (e.g. far corner of the ground plane).

3. Wait ~5 seconds for `DayNightCycle` to advance from Dawn. Confirm `WorldManager` persists `TimeOfDay` (fires on phase change).

4. Press **Space** 3 times to dirty the trait profile.

5. Manually call save: In the Console, you can add a temporary key binding (or call from `TraitDebugHUD`) that calls:

   ```csharp
   ServiceLocator.Get<ISaveManager>().MarkDirty();
   await ServiceLocator.Get<ISaveManager>().SaveAsync();
   ```

   Alternatively, add a **quick save** key (e.g. `F5`) to `InputHandler` for dev builds.

6. Stop Play mode.

7. Open `Application.persistentDataPath/save.json` in a text editor (macOS path: `~/Library/Application Support/DefaultCompany/ProjectFenrir/save.json`). Confirm:
   - `"Version": "0.1.0"` present
   - `"TimeOfDay"` is not 0 (advanced from Dawn)
   - Trait values reflect the dodges taken in step 4

8. Enter Play mode again.  
   Expected: `Bootstrap.LoadSaveData()` reads the file, `WorldManager.Start()` calls `DayNightCycle.InitializeFromSave(TimeOfDay)`, trait values match what was saved.

9. Confirm in Console: `[Bootstrap] Save loaded: True`.

**Acceptance criteria:**

- `save.json` written to `persistentDataPath` with correct schema
- Traits persisted and restored (verify 3 dodge deltas in saved profile)
- `TimeOfDay` non-zero and matches pre-stop value
- `[Bootstrap] Save loaded: True` in Console
- No exceptions during load

**Branch:** `feature/save-roundtrip`

---

### P2-06 · Quick-save dev key + F5 binding

**Goal:** Developer can trigger save at will during testing without modifying production code.

**Steps:**

1. In `InputHandler.ProcessKeyboard()`, add inside `#if UNITY_EDITOR || DEVELOPMENT_BUILD`:

   ```csharp
   if (kb.f5Key.wasPressedThisFrame)
   {
       if (ServiceLocator.TryGet<ISaveManager>(out ISaveManager save))
       {
           save.MarkDirty();
           _ = save.SaveAsync();
           Debug.Log("[Dev] Manual save triggered.");
       }
   }
   ```

2. Test: Press F5 in Play mode, confirm `save.json` updates.

**Acceptance criteria:**

- F5 in Play mode writes `save.json`
- Key is stripped in non-dev builds (guarded by `#if`)

**Branch:** `feature/save-roundtrip` (same branch as P2-05)

---

### P2-07 · iOS build target setup

**Goal:** Project builds to iOS device without errors. Bundle ID, signing, and native Swift plugins confirmed working.

**Steps:**

1. **Switch platform:**  
   File → Build Settings → iOS → Switch Platform. Wait for reimport.

2. **Set Bundle Identifier:**  
   Edit → Project Settings → Player → iOS tab → Bundle Identifier = `com.fenrir.game` (matches App Store reservation — update if needed).

3. **Set minimum iOS version:**  
   Player → iOS → Other Settings → Target minimum iOS Version = `16.0`.

4. **Check scripting backend:**  
   Player → iOS → Other Settings → Scripting Backend = **IL2CPP**. Architecture = **ARM64** only (drop ARMv7 — not needed for iOS 16+).

5. **Verify Swift plugins:**  
   `Assets/Plugins/iOS/KeychainPlugin.swift` and `iCloudPlugin.swift` must have Platform settings:
   - Platform: iOS
   - Don't process: unchecked
   Confirm in Inspector when file is selected.

6. **Build:**  
   File → Build Settings → Build. Choose output folder `src/ProjectFenrir/Builds/iOS/`. Unity generates an Xcode project.

7. **Open in Xcode:**  
   Open `Unity-iPhone.xcodeproj`. Select your development team in Signing & Capabilities. Build to connected device or simulator.

8. **Verify Keychain:**  
   On first launch, `ElementSeedService` calls `KeychainBridge.GetSeed(slotKey)`. Confirm no crash and `null` returned (empty Keychain = new player, goes to Awakening).

9. **Check that Awakening scene loads** (player has not awakened, so `Bootstrap.RouteToInitialScene` calls `SceneRouter.LoadAwakeningAsync()`).

**Acceptance criteria:**

- Unity iOS build completes with 0 errors
- Xcode project builds and signs cleanly
- App launches on device/simulator without crash
- Keychain call returns gracefully (no crash on nil)
- Awakening scene loads correctly
- No `Application.isEditor` code paths executing on device

**Branch:** `chore/ios-build-setup`

---

### P2-08 · Merge Phase 2 → `main`

**Goal:** Clean `main` branch with all Phase 2 work merged via PR.

**Steps:**

1. Ensure all P2 branches are merged into `chore/repo-setup`:

   ```bash
   git checkout chore/repo-setup
   git merge feature/trait-debug-hud
   git merge feature/first-enemy
   git merge feature/save-roundtrip
   git merge chore/ios-build-setup
   ```

2. Run `git push origin chore/repo-setup`.
3. On GitHub: open a Pull Request from `chore/repo-setup` → `main`.
4. PR description must include:
   - Summary of what changed
   - Screenshot of TraitDebugHUD showing live trait values
   - Screenshot of Console with `LightAttackLandedEvent` log
   - Confirmation: all 27 EditMode tests green
5. Merge (squash or merge commit — your choice).
6. Tag: `git tag v0.2.0-alpha && git push origin v0.2.0-alpha`.
7. Update `CHANGELOG.md` with Phase 2 entries.

**Acceptance criteria:**

- `main` branch has passing CI (lint + EditMode tests)
- `v0.2.0-alpha` tag exists on GitHub
- `CHANGELOG.md` updated

---

## Phase 3 — MVP Gameplay

**Target:** Playable loop from Awakening → Ember Forest exploration → evolution → Emberlord → credits.  
**Branch strategy:** One feature branch per ticket, all merged to `develop` branch, then `develop` → `main` at phase end.

---

### P3-01 · Scene architecture: Bootstrap → Awakening → EmberForest flow

**Goal:** Full scene routing works end-to-end. New game starts Awakening. Existing save skips to EmberForest.

**Steps:**

1. Wire Awakening scene:
   - Add `AwakeningSequencer` component to a root GameObject.
   - Create placeholder UI panels: `CharacterCreationPanel`, `ElementRevealPanel`, `LoadingPanel` (use Unity Canvas/Panel with `TextMeshProUGUI` labels for now).
   - Wire panels to `AwakeningSequencer` SerializedFields via Inspector.
2. Wire Bootstrap scene:
   - Run **Fenrir → Setup Scenes → Setup Bootstrap Scene** editor script.
   - Confirm `Bootstrap`, `AppStateMachine`, `GameLoop` all on `FenrirManager` GameObject.
   - Confirm EventSystem uses `InputSystemUIInputModule`.
3. Verify routing:
   - Delete `save.json` from `persistentDataPath`. Play Bootstrap scene → should load Awakening.
   - Complete Awakening → should load EmberForest.
   - Stop. Re-play. Should skip Awakening, load EmberForest directly.

**Acceptance criteria:**

- New player sees Awakening scene (no save file)
- Returning player loads EmberForest directly
- No `SceneManager` calls anywhere except inside `SceneRouter`

**Branch:** `feature/scene-routing`

---

### P3-02 · Character creation UI

**Goal:** Player can enter name, select gender, choose face preset, choose body type. Tapping **Confirm** stores data and proceeds.

**Steps:**

1. Design the panel layout in Unity UI:
   - Name field: `TMP_InputField`
   - Gender toggle: two `Toggle` buttons (Male / Female) in `ToggleGroup`
   - Face preset: horizontal `ScrollRect` with 4 placeholder image buttons
   - Body type: same pattern, 2 options
   - Confirm button: `Button` with `TextMeshProUGUI` label
2. Create `CharacterCreationUI.cs` in `Assets/_Project/Scripts/UI/`:
   - Reads values from all input controls
   - On Confirm: validates name not empty
   - Calls `AwakeningSequencer.OnCharacterCreationComplete(name, gender, facePreset, bodyType)`
3. `AwakeningSequencer.OnCharacterCreationComplete()` writes to `SaveManager.Current.Character` and calls `MarkDirty()`.
4. Proceed to element reveal.

**Acceptance criteria:**

- All fields functional
- Empty name shows error message (not a crash)
- `SaveData.Character.Name` populated correctly after confirm
- Works on 375px width (iPhone SE minimum) — test in Game View with 390×844 resolution

**Branch:** `feature/character-creation`

---

### P3-03 · Awakening ceremony — element reveal and seed

**Goal:** `ElementSeedService` derives element from Keychain seed. Element displayed on `ElementRevealPanel`. Player proceeds to game.

**Steps:**

1. In `AwakeningSequencer.BeginAwakening()` (called after CharacterCreation):
   - Call `ElementSeedService.GetOrCreateSeed(slotIndex)` — reads from Keychain or generates new seed.
   - Call `ElementSeedService.DeriveElement(seed)` → returns `Element`.
   - Write element to `save.Current.Character.CurrentElement`.
   - Mark `save.Current.Character.HasAwakened = true`.
   - Call `save.MarkDirty()` + `await save.SaveAsync()`.
2. Create `ElementRevealUI.cs`:
   - Reads `SaveManager.Current.Character.CurrentElement`
   - Shows element name in large text (e.g. "FIRE")
   - Shows placeholder element icon (use a colored square for now)
   - 3-second hold (configurable via `_elementRevealHoldSeconds`)
3. Phase 1 MVP: `MvpCommonOnly = true` — all elements fall back to `Common` tier.  
   Confirm: any seed produces Fire, Water, Earth, or Air.

**Acceptance criteria:**

- `HasAwakened` flag set in saved JSON after reveal
- Element displayed on screen for 3 seconds
- Element is one of {Fire, Water, Earth, Air} in MVP
- Seed persisted in Keychain (confirm by deleting `save.json` and replaying — same element returned)

**Branch:** `feature/awakening-ceremony`

---

### P3-04 · Ember Forest — zone layout

**Goal:** 200×200 ground plane replaced with proper Ember Forest terrain layout. Four sub-zones walkable. Visual separation using basic Unity primitives (colored planes/walls — art comes in Phase 4).

**Sub-zones (from WORLD_DESIGN.md):**

| Zone | Description |
| --- | --- |
| The Threshold | Entry zone, tutorial encounters, Village |
| The Ashwood | Dense forest, patrol enemies, lore fragments |
| The Ember Fields | Open area, Ash Wolf packs, shrine |
| Char Ravine | Bottleneck, Emberlord approach, doctrine shrine |

**Steps:**

1. Replace flat ground plane with 4 sub-planes arranged in a 2×2 grid:
   - Each sub-zone: 80×80 units, unique material color (grey, brown, orange-red, dark)
   - Low walls/cubes as zone boundaries (passable gaps for transitions)
2. Place zone-transition trigger volumes (`BoxCollider` with `isTrigger = true`):
   - Add `ZoneTrigger.cs` — on enter, calls `RegionLoader.LoadSubZone(zoneId)`
   - `RegionLoader.cs` exists but is a stub — add minimal implementation: log + fire `AreaRevisitedEvent` on re-entry
3. Place Player spawn in The Threshold center.
4. Add Directional Light with warm orange tint to evoke ember aesthetic.
5. Bake NavMesh across all 4 sub-planes.

**Acceptance criteria:**

- All 4 sub-zones walkable without falling through
- Zone transition triggers fire `AreaRevisitedEvent` on second entry
- NavMesh covers all walkable area
- No Z-fighting on terrain

**Branch:** `feature/ember-forest-layout`

---

### P3-05 · All 5 enemy types as prefabs

**Goal:** 5 enemy Prefabs in `Assets/_Project/Prefabs/Enemies/` — one per archetype. Each has correct stats from `EnemyDefinitions.json`.

**Enemy roster (from COMBAT_SYSTEM.md):**

| ID | Name | Archetype | Element | HP | Detection | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| `ash_wolf` | Ash Wolf | Pack | Fire | 80 | 8 | Patrols in groups |
| `flame_sprite` | Flame Sprite | Ranged | Fire | 45 | 12 | Projectile placeholder |
| `ember_stag` | Ember Stag | Defensive | Fire | 120 | 6 | Shield behaviour |
| `ash_revenant` | Ash Revenant | Elite | Fire | 180 | 10 | Telegraphed heavy |
| `inferno_wisp` | Inferno Wisp | Aggressive | Fire | 60 | 15 | High speed |

**Steps:**

1. Create `Assets/_Project/Prefabs/` folder.
2. For each enemy:
   - Create a Capsule in scene, rename to enemy name
   - Add components: `EnemyBase`, `EnemyHealth`, `EnemyAI`, `EnemyCombat`, `EnemyTraitEmitter`, `NavMeshAgent`
   - Set stats per table above
   - Drag from Hierarchy to `Assets/_Project/Prefabs/Enemies/` to create Prefab
3. Create `EnemySpawner.cs`:
   - Reads `EnemyDefinitions.json` from StreamingAssets
   - Spawns prefabs at designated spawn points in scene
   - Has a `[ContextMenu("Spawn All")]` for editor testing
4. Place spawn points in Ashwood and Ember Fields sub-zones.
5. Verify all 5 enemy types spawn, patrol, and can be killed.

**Acceptance criteria:**

- 5 Prefabs exist and load without missing component warnings
- Stats match `EnemyDefinitions.json` — verify by reading file and checking Inspector
- All 5 patrol correctly using NavMesh
- `EnemyTraitEmitter` fires appropriate trait events on player kill (confirm in Console)

**Branch:** `feature/enemy-prefabs`

---

### P3-06 · Combat system — full loop

**Goal:** Full combat encounter works: player attacks, enemy attacks back, HP depletes on both sides, death triggers correct trait events.

**Steps:**

1. **Player takes damage:**  
   `EnemyCombat.Attack()` → `AttackResolver.Resolve()` → `PlayerHealth.TakeDamage()` → `PlayerHealth` emits `HitTakenNoDodgeEvent` if player didn't dodge.

2. **Dodge mechanic:**  
   `InputHandler`: map **Space** to dodge in `TouchMapper.OnDodgeInput()` → `PlayerCombat.Dodge()`.  
   Dodge activates a 0.4s invincibility window (`GameConfig.DodgeInvincibilityDuration`). During this window, `AttackResolver.Resolve()` returns 0 damage.  
   Emit `DodgeUsedEvent` on every dodge, regardless of whether it blocked damage.

3. **Block mechanic:**  
   Map **Left Shift** to block. While held: `PlayerCombat.IsBlocking = true`.  
   `AttackResolver`: if `IsBlocking`, reduce incoming damage by 50%. If attack landed within first 0.2s of block start = `PerfectBlockEvent`.

4. **Death classification:**  
   `PlayerHealth.OnDeath()` must determine death type (in priority order):
   - Ambush: `Time.time - CombatContext.CombatStartTime < 5f` → `DeathAmbushEvent`
   - Pattern Fail: `CombatContext.DeathsVsThisEnemy >= 3` → `DeathPatternFailEvent`
   - Reckless: `HitsTakenWithoutDodge >= 3 && DodgesUsedThisFight == 0` → `DeathRecklessEvent`
   - Sacrifice: `PlayerHealth.HP / MaxHP < GameConfig.EnergyLowThreshold && was attacking` → `DeathSacrificeEvent`
   - Fallback → `DeathOverwhelmedEvent`

5. **Currency loss on death:**  
   `PlayerHealth.OnDeath()`: deduct 15–25% of `SaveData.Character.Currency` (use `GameConfig.DeathCurrencyLossMin/Max`). Mark dirty.

6. **Respawn:**  
   For MVP: respawn at last checkpoint (`SaveData.World.LastCheckpointId`). Checkpoint = The Threshold center. Restore full HP.

7. **Enemy combat AI:**  
   `EnemyAI.Update()` in Attack state: call `EnemyCombat.Attack(player)` when within `AttackRange` and `AttackCooldown` elapsed.

**Acceptance criteria:**

- Player takes damage from enemy (HP bar decreases)
- Dodge makes player invincible for 0.4s (attack passes through)
- `DodgeUsedEvent` fires → Patience shifts in TraitDebugHUD
- Death triggers correct classification (test each type manually)
- Currency reduced 15–25% on death (check save.json)
- Respawn at checkpoint with full HP
- 5 consecutive kills without dying → enemy death but no player trait events (only `BossKilledEvent` on boss)

**Branch:** `feature/combat-full-loop`

---

### P3-07 · Evolution shrine system

**Goal:** Player approaches a shrine, `ShrineController` checks eligibility, triggers evolution sequence if thresholds met.

**Shrine definition (from EVOLUTION_SYSTEM.md):**

- Tier 1 evolution is permanent. Checked once, never changed.
- Requires: element match + trait thresholds met on ≥1 eligible evolution.
- FitScore: sum of `(traitValue - threshold) / threshold` for all passing traits.

**Steps:**

1. Create `Assets/_Project/Prefabs/World/EvolutionShrine.prefab`:
   - Capsule or cube mesh (placeholder)
   - `ShrineController` component
   - `SphereCollider` (isTrigger, radius = 2.5)
   - Set `ShrineController._shrineId = "shrine_001"`
2. Place shrine in Ember Fields sub-zone.
3. Wire `ShrineController.OnTriggerEnter()`:
   - Only responds to Player tag
   - Calls `IEvolutionChecker.Check(profile, element)` via `ServiceLocator`
   - If no candidates: show brief "not yet" journal hint
   - If candidates: call `EvolutionSequencer.Begin(candidate)`
4. Implement `EvolutionSequencer.Begin(candidate)`:
   - Fade screen to white (2s) — `Camera.main` `postprocessing` or simple image overlay
   - Apply evolution: `SaveData.Character.CurrentEvolution = candidate.EvolutionId`
   - Call `TraitProfile.ApplyEvolutionCarryover()` (30% toward neutral)
   - Reset session event counts
   - Call `JournalController.AddEvolutionEntry(candidate.EvolutionId)` → vague flavour text
   - Mark save dirty + save async
   - Fade back in
5. Test: manually set Patience to 80 via Console command or test utility, approach shrine, verify evolution fires.

**Acceptance criteria:**

- Shrine triggers only for Player tag
- Not-eligible player: hint message, no evolution
- Eligible player: fade, evolution applied, journal entry added
- `CurrentEvolution` set in save.json after evolution
- Traits at 30% carryover toward neutral (e.g. Patience was 80 → now 74)
- Second approach to shrine: nothing happens (Tier 1 is permanent)

**Branch:** `feature/evolution-shrine`

---

### P3-08 · Journal system

**Goal:** Player can open Journal (J key in Editor / tap in iOS), see vague lore entries after evolution.

**Steps:**

1. `JournalController.cs` exists with `EvolutionEntries` dictionary. Implement:
   - `Toggle()` called on J key press (add to `InputHandler`)
   - Instantiate `_entryPrefab` (TextMeshProUGUI) per entry in `_entryContainer`
   - Populate from `SaveData.Character.JournalEntries` on open
2. Create minimal journal panel in EmberForest HUD Canvas:
   - Dark semi-transparent background
   - ScrollRect for entries
   - Close button
3. Persist entries: `JournalController.AddEntry(text)` appends to `SaveData.Character.JournalEntries` and calls `MarkDirty()`.

**Acceptance criteria:**

- J key opens/closes journal
- Evolution entry visible after evolution event (correct flavour text)
- Journal persists across Play sessions
- Scroll works if entries exceed panel height

**Branch:** `feature/journal-ui`

---

### P3-09 · Day/Night cycle visible

**Goal:** Sky colour and ambient light visibly change across 20-minute cycle. Directional light rotates. `NightExplorationEvent` fires when Night begins.

**Steps:**

1. Open EmberForest scene. Select Directional Light.
2. Add `DayNightCycle` component references:
   - `_directionalLight` = scene Directional Light
   - Assign light colour gradient in Inspector (Dawn: warm orange, Day: bright white, Dusk: deep red, Night: deep blue)
3. Verify `DayNightCycle.Update()` rotates light from 0° to 360° over `GameConfig.DayNightCycleDurationSeconds = 1200f`.
4. For testing: temporarily set `DayNightCycleDurationSeconds = 30f` in `GameConfig.cs`, verify full cycle completes in 30 seconds. Restore to 1200f before commit.
5. Confirm `NightExplorationEvent` fires when phase transitions to Night:
   - Subscribe a debug listener in `Bootstrap` temporarily
   - Watch Console for the event

**Acceptance criteria:**

- Sky/ambient light changes visibly across cycle phases
- `NightExplorationEvent` fires in Console at Night phase
- `WorldManager` persists `TimeOfDay` to save on phase change (verify in save.json)
- `DayNightCycleDurationSeconds = 1200f` in committed code

**Branch:** `feature/day-night-visible`

---

### P3-10 · Main menu

**Goal:** Simple main menu scene: "New Game", "Continue", "Quit" buttons. Replaces placeholder Bootstrap direct-to-Awakening routing.

**Steps:**

1. Create MainMenu scene (add to Build Settings at index 1; shift other scenes).
2. Create `MainMenuUI.cs`:
   - `OnNewGame()`: delete save, call `SceneRouter.LoadAwakeningAsync()`
   - `OnContinue()`: check if save exists; if yes, `SceneRouter.LoadGameAsync()`; if no, grey out button
   - `OnQuit()`: `Application.Quit()`
3. Add title text "FENRIR" in large font.
4. Update `Bootstrap.RouteToInitialSceneAsync()`: if no awakened save, go to MainMenu (not directly to Awakening).

**Acceptance criteria:**

- New Game → Awakening scene
- Continue (with save) → EmberForest
- Continue button greyed out when no save
- Quit works on device (not in Editor — expected)

**Branch:** `feature/main-menu`

---

### P3-11 · Emberlord boss (Phase 1 of 3)

**Goal:** Emberlord spawns in Char Ravine. Three-phase fight with increased damage between phases. `BossKilledEvent` fires on death.

**Design note:**  
Full boss implementation (animations, VFX, unique move patterns) is Phase 4. Phase 3 goal is functional mechanics only — correct HP thresholds, phase transitions, `BossKilledEvent` on death, world state updated.

**Steps:**

1. Create `EmberlordController.cs` in `Assets/_Project/Scripts/Entities/Enemies/`:
   - Extends from `EnemyBase` + uses `EnemyHealth`, `EnemyCombat`, `EnemyAI`
   - `Phase` enum: Phase1, Phase2, Phase3
   - Phase transitions at 66% and 33% HP thresholds
   - Each phase: increase `EnemyCombat.AttackDamage` by 20% and `EnemyAI.MoveSpeed` by 15%
   - `OnDeath()`: `BehaviorEventBus.Emit(new BossKilledEvent())`, set `SaveData.World.EmberlordDefeated = true`, mark dirty
2. Place Emberlord Prefab in Char Ravine, behind a trigger barrier that only opens once player reaches that area.
3. Create `VictorySequencer.cs`:
   - Subscribes to `BossKilledEvent`
   - Shows "VICTORY" text overlay
   - 3-second hold
   - Shows credits text (placeholder: "Made by Sen1F")
   - Returns to Main Menu

**Acceptance criteria:**

- Emberlord transitions phases at correct HP thresholds (visible via debug log)
- `BossKilledEvent` fires → `Decisive` trait shifts in TraitDebugHUD
- `EmberlordDefeated = true` in save.json after kill
- Victory screen appears

**Branch:** `feature/emberlord-boss`

---

### P3-12 · Phase 3 merge + tag

1. All P3 feature branches merged to `develop`.
2. Full playthroughs: new game → awakening → explore → evolve → boss.
3. All 27 EditMode tests still green.
4. iOS build clean.
5. Tag `v0.3.0-alpha`.
6. Update `CHANGELOG.md`.

---

## Phase 4 — Polish

**Branch strategy:** `polish/` prefix branches. Merge to `develop`, then `main` at phase end.

---

### P4-01 · Art pass — player and enemies

- Replace capsule player with actual character mesh (rigged humanoid, `.fbx`).
- Add Animator component with placeholder state machine: `Idle`, `Walk`, `Attack`, `Dodge`, `Death`.
- Animator Controller: transitions driven by `PlayerState` state machine.
- Same for each of the 5 enemies.
- Emberlord: separate rig with 3-phase materials (base, scorched, molten).

---

### P4-02 · Animation — player

| Animation | Trigger | Notes |
| --- | --- | --- |
| Idle | Default | Breathing loop |
| Walk | `IsMoving == true` | Blend with velocity magnitude |
| Light Attack | `LightAttack` trigger | 0.4s, hit frame at 0.25s |
| Heavy Attack | `HeavyAttack` trigger | 0.7s, hit frame at 0.5s |
| Dodge | `Dodge` trigger | 0.4s, invincible 0–0.4s |
| Block | `IsBlocking` bool | Sustained hold pose |
| Hit | `TakeHit` trigger | 0.2s stagger |
| Death | `IsDead` bool | Fall and settle, 1.5s |
| Evolution burst | `Evolve` trigger | One-shot VFX transition (1.5s) |

All animations: sync `PlayerState` transitions to Animator triggers in `PlayerController.Update()`.

---

### P4-03 · Animation — enemies

| Enemy | Animations needed |
| --- | --- |
| Ash Wolf | Idle, Walk, Attack, Death |
| Flame Sprite | Idle, Float, Shoot, Death |
| Ember Stag | Idle, Walk, Charge, Death |
| Ash Revenant | Idle, Walk, Heavy (telegraphed), Death |
| Inferno Wisp | Idle, Dash, Death |
| Emberlord | All above + Phase transitions (3 variants) |

---

### P4-04 · VFX

| Effect | Implementation |
| --- | --- |
| Element hit flash | Particle burst, colour-coded by element |
| Dodge trail | Motion trail renderer, 0.4s, element colour |
| Evolution burst | Full-screen radial VFX, 2s, element colour |
| Shrine glow | Particle system, idle + activated state |
| Emberlord phase transition | Screen shake + particle eruption |
| Day/night sky | Skybox material with gradient shader driven by `DayNightCycle.NormalizedTime` |
| Death dissolve | Dissolve shader on enemy mesh, 1.5s |

---

### P4-05 · Audio

**Music layers (adaptive):**

| State | Track | Notes |
| --- | --- | --- |
| Dawn/Day | Ambient forest | Calm, textured |
| Dusk | Tension ambient | Unease, sparse melody |
| Night | Danger ambient | Rhythmic, low |
| Combat | Combat layer | Layered over ambient, fades on exit |
| Boss | Boss theme | Replaces ambient entirely |
| Evolution | Evolution sting | One-shot 4-bar stinger |

**SFX:**

- Player: footsteps, light/heavy attack whoosh, dodge swish, block clang, perfect block ring, hit grunt, death
- Enemies: growl, attack, death
- UI: menu confirm, cancel, journal open/close
- World: shrine pulse, day-phase bell

**Implementation:**  
All audio routed through `AudioManager`. Combat layer uses `MusicLayer.CrossfadeTo()`. `SFXPool` handles all short one-shot sounds. Placeholder tracks: royalty-free during development; replace before App Store submission.

---

### P4-06 · Haptics (iOS)

Add `HapticsManager.cs` in `Assets/_Project/Scripts/Audio/`:

- Wraps `UnityEngine.iOS.Taptic` or native UIImpactFeedbackGenerator via Swift plugin
- Calls: `Light` on dodge, `Medium` on hit, `Heavy` on perfect block, `Rigid` on death, `Soft` on evolution

---

### P4-07 · Elemental resistance tuning

From `COMBAT_SYSTEM.md`:

- Same element: −25% damage
- Opposite element: +25% damage
- Non-ability hits: ignore element

Tuning pass: run 10 fights per enemy type, log damage dealt/received. Adjust `AttackData.BaseDamage` per enemy until fights feel appropriately difficult for the pacing target (Ash Wolf: 3–5 hits to kill player; Emberlord: ~25 hits at Phase 1).

---

### P4-08 · Trait weight tuning

After 5+ full playthroughs:

- Log all trait deltas from `EventLogger`
- Identify traits stuck near 50 (no content to move them) vs traits that spike too fast
- Adjust weights in `TraitWeights.json` (do NOT change C# defaults)
- Verify evolution thresholds remain reachable within ~2 hours of play

---

### P4-09 · Performance profiling

Targets:

- 60 FPS on iPhone 15 (sustained 20-minute session)
- < 400 MB RAM at runtime
- < 2-second load times per scene
- No thermal throttling within first 15 minutes

**Tools:**

- Unity Profiler (CPU/GPU/Memory tabs)
- Xcode Instruments (Energy Log, Allocations)
- Frame Debugger (overdraw, draw call count)

**Common iOS culprits to check:**

- Draw calls > 100 per frame (use GPU instancing, static batching)
- Per-frame allocations (GC pressure from `new` in Update loops)
- NavMesh pathfinding frequency (reduce `EnemyAI` update rate to 5 Hz using `InvokeRepeating`)
- Texture compression: ensure all textures use ASTC (iOS default for URP Mobile)

---

### P4-10 · Accessibility

- Dynamic text size: support iOS Dynamic Type for all UI text
- Color-blind modes: offer 3 element icon variants (none required for MVP — mark as `v1.1` stretch)
- Reduce motion option: disable screen shake, evolution fade (accessibility setting in Options menu)

---

## Phase 5 — Release Readiness

---

### P5-01 · App icon + launch screen

- 1024×1024 app icon (Fenrir wolf motif — final art)
- Launch screen: black background, FENRIR wordmark centred
- Export all required sizes via Unity or Xcode asset catalog

---

### P5-02 · Privacy policy + data handling

- Hosted URL required for App Store listing (e.g. GitHub Pages)
- Content: no personal data collected, no analytics sent offdevice, Keychain usage for seed only
- Add privacy manifest (`PrivacyInfo.xcprivacy`) — required by Apple since iOS 17.
  - Declare Keychain usage reason: `NSPrivacyAccessedAPICategoryUserDefaults` — N/A
  - Declare Keychain reason: custom reason string

---

### P5-03 · Crash handling + symbolication

- Enable Xcode crash reporting in Organizer
- Add a crash handler that saves the current `save.json` path to a recovery file before crash (best-effort)
- Symbol upload: ensure Unity dSYM files uploaded with each TestFlight build

---

### P5-04 · TestFlight build

1. Bump version to `1.0.0` in Player Settings.
2. Archive in Xcode (Product → Archive).
3. Upload to App Store Connect.
4. Distribute to internal testers (yourself + up to 25 internal).
5. External beta requires App Store Review approval.

**Pre-TestFlight checklist:**

- All P3 content complete (playable loop)
- No `Debug.Log` calls in release builds (use `EventLogger` with `verbose: false`)
- `TraitDebugHUD` stripped (confirm `#if DEVELOPMENT_BUILD` guard)
- Bundle ID matches App Store Connect
- No placeholder assets (temp cubes, unlabeled textures)
- iCloud save backup opt-in works
- Keychain seed persists across uninstall? (No — seed lost on uninstall, new element assigned. This is by design — document it)

---

### P5-05 · Beta feedback pass

- 2-week TestFlight beta
- Collect: crash reports, session length, evolution achievement rate
- Fix P1/P2 (crash) bugs immediately
- Defer cosmetic feedback to `v1.1`

---

### P5-06 · App Store submission

1. App Store Connect: fill metadata (name, subtitle, description, keywords, category = Games → Role Playing).
2. Screenshots: 6.5", 5.5", 12.9" iPad (if supporting).
3. App preview video: 30s gameplay clip.
4. Age rating: 9+ (fantasy violence).
5. Submit for review.
6. Target: within 24–48 hours of review submission.

---

### P5-07 · `v1.0` release

1. Tag `v1.0.0` on `main`.
2. Merge `develop` → `main`.
3. Update `CHANGELOG.md` with full release notes.
4. Update `ROADMAP.md` — mark Phase 5 complete.
5. Update `DEVELOPMENT_DIARY.md` — final session entry.

---

## Git strategy overview

```text
main          ← stable releases only (tagged)
develop       ← integration branch for Phase 3+
  feature/*   ← individual feature tickets
  fix/*        ← bug fixes
  chore/*      ← infra, tooling, scene setup
  polish/*     ← Phase 4 art/audio/VFX branches
  release/*    ← release prep (Phase 5)
```

All PRs require:

- 0 compile errors
- 0 failing EditMode tests
- At least one screenshot or Console log proving acceptance criteria

Commit format (enforced by hook): `type(scope): subject`  
Bump `CHANGELOG.md` on every merge to `main`.

---

## Risk register

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Unity 6 API breaks between LTS patches | Low | Medium | Pin all package versions in manifest.json |
| Keychain access denied on device (entitlements) | Medium | High | Test Keychain write/read as P2-07 acceptance criterion |
| NavMesh pathfinding fails on complex terrain | Medium | Medium | Keep terrain simple until Phase 4 art pass |
| Trait thresholds unreachable with real play patterns | Medium | High | EventLogger analytics → adjust weights in P4-08 |
| Evolution is permanent — player regret | Low | High | Vague journal + 30% carryover softens blow; design already accounts for this |
| App Store rejection (missing privacy manifest) | Medium | High | P5-02 creates `PrivacyInfo.xcprivacy` before submission |
| Save data lost on device uninstall | High | Medium | Document expected behaviour; add opt-in iCloud backup in P3 |
| Two source trees (`src/Assets` vs `src/ProjectFenrir`) causing confusion | ~~Resolved~~ | — | P2-01 done 2026-06-11 — `src/Assets` + `src/Tests` deleted, live tree confirmed newer |

---

*This document is the single source of truth for planned engineering work. Update it as tickets are completed and new work is discovered.*
