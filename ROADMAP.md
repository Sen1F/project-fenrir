# Roadmap

## Phase 0 — Game Definition ✅ COMPLETE

- [x] Game Design Document v0.1
- [x] World Lore v0.1 (The Fenrir Principle)
- [x] Awakening System Design (locked)
- [x] Hidden Trait System Design
- [x] Evolution System Design (MVP evolutions defined)
- [x] Combat System Design (Emberlord boss designed)
- [x] World Design — Ember Forest (sub-zones, shrines, NPCs)
- [x] All design questions closed

## Phase 1 — Repo / Foundation ✅ COMPLETE

**Engine:** Unity 6 LTS | **Language:** C# | **Pipeline:** URP

- [x] Unity 6 LTS project created, URP (Mobile renderer) configured
- [x] New Input System installed and set as active input handler
- [x] `_Project/` folder structure scaffolded
- [x] Bootstrap scene + ServiceLocator skeleton
- [x] `TraitProfile.cs` + `BehaviorEvent.cs` data models
- [x] `BehaviorEventBus.cs` (with Unsubscribe support)
- [x] `KeychainBridge.cs` + `KeychainPlugin.swift` stub
- [x] `iCloudPlugin.swift` stub
- [x] `TraitWeights.json` + `EvolutionSignatures.json` + `EnemyDefinitions.json`
- [x] `AudioManager.cs` + `MusicLayer.cs` + `SFXPool.cs`
- [x] EditMode unit tests — 27 tests, 5 suites, all passing
- [x] iOS bundle ID set: `com.fenrir.projectfenrir`
- [x] CI: GitHub Actions — lint-docs + unity-build + editmode tests
- [x] First playable: player movement, Cinemachine camera, EmberForest scene wired

## Phase 2 — Architecture 🔄 IN PROGRESS

- [x] `SceneRouter` — async typed scene loading, App/GameState events
- [x] `AppStateMachine` — PlayerState transitions, valid-transition table
- [x] `GameLoop` — pause/resume, auto-save on app background/quit
- [x] `Bootstrap` — StreamingAssets JSON config loading, SceneRouter routing
- [x] `CombatContext` — active encounter tracking, trait events on combat end
- [x] `ServiceLocator.Unregister<T>()` added
- [x] PlayerController (movement, Cinemachine camera) — first playable ✅
- [x] InputHandler + GestureRecognizer + TouchMapper wired
- [x] EnemyBase + EnemyAI (NavMesh) — code complete, editor spawn script done
- [x] CombatSystem + AttackResolver + HitStateManager — code complete
- [x] TraitAccumulator + EvolutionChecker — code complete + unit tested
- [x] ShrineController — code complete
- [x] DayNightCycle — in EmberForest scene
- [x] SaveManager (JSON) — code complete + unit tested
- [x] AudioManager + MusicLayer + SFXPool — wired to DayNightCycle
- [x] HUD — EnergyBar + health bar code complete
- [ ] NavMesh baked in EmberForest
- [ ] First enemy alive in Play mode (Ash Wolf patrol + chase)
- [ ] Trait events firing live (dodge → Patience ↑ confirmed in logs)
- [ ] Save/Load round-trip verified in Play mode
- [ ] Bootstrap scene fully wired in Unity Inspector

## Phase 3 — MVP Gameplay

- [ ] Awakening ceremony scene (element reveal, reroll)
- [ ] Main menu + character creation (name, gender, face, body)
- [ ] Ember Forest — The Threshold sub-zone
- [ ] Ember Forest — The Ashwood sub-zone
- [ ] Ember Forest — The Ember Fields sub-zone
- [ ] Ember Forest — Char Ravine sub-zone
- [ ] 5 enemy types (Ash Wolf, Flame Sprite, Ember Stag, Ash Revenant, Inferno Wisp)
- [ ] Emberlord boss (3-phase)
- [ ] Hidden trait tracking live
- [ ] Evolution shrine system + Tier 1 evolution sequence
- [ ] Doctrine shrine + lore fragment
- [ ] Village NPCs (Elder, Friend, Rival)
- [ ] Village merchant
- [ ] Day/night cycle live
- [ ] Journal (post-evolution entry)
- [ ] Local save + iCloud backup opt-in
- [ ] Win condition (boss defeated, barrier open)

## Phase 4 — Polish

- [ ] Animations (player, enemies, creatures, evolution sequence)
- [ ] VFX (elemental effects, evolution burst, shrine glow states)
- [ ] SFX + ambient audio
- [ ] Haptics (iOS)
- [ ] Elemental resistance tuning
- [ ] Trait weight tuning (playtest data)
- [ ] Evolution threshold tuning
- [ ] Difficulty balancing
- [ ] Performance profiling (60fps target, thermal testing)
- [ ] Memory audit
- [ ] Accessibility (dynamic text, colour-blind modes)

## Phase 5 — Release Readiness

- [ ] App icon + launch screen
- [ ] Privacy policy
- [ ] App Store screenshots + preview video
- [ ] Crash handling (symbolication)
- [ ] TestFlight build
- [ ] Beta feedback pass
- [ ] `v1.0` tag on `main`
