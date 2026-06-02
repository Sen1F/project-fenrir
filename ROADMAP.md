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

## Phase 1 — Repo / Foundation 🔄 IN PROGRESS

**Engine:** Unity 2022 LTS | **Language:** C# | **Pipeline:** URP

- [ ] Unity project created, URP configured
- [ ] New Input System installed
- [x] `_Project/` folder structure scaffolded
- [x] Bootstrap scene + ServiceLocator skeleton
- [x] `TraitProfile.cs` + `BehaviorEvent.cs` data models
- [x] `BehaviorEventBus.cs`
- [x] `KeychainBridge.cs` + `KeychainPlugin.swift` stub
- [x] `iCloudPlugin.swift` stub
- [x] `TraitWeights.json` + `EvolutionSignatures.json` + `EnemyDefinitions.json` config placeholders
- [x] `AudioManager.cs` + `MusicLayer.cs` + `SFXPool.cs` stubs
- [x] EditMode unit tests (TraitAccumulator, EvolutionChecker, ElementDistribution, TraitProfile)
- [ ] iOS build target configured (bundle ID, signing)
- [ ] CI: build validation on push to `develop`

## Phase 2 — Architecture

- [ ] GameLoop, StateMachine, SceneRouter
- [ ] PlayerController (movement, camera)
- [ ] InputHandler (joystick + swipe mapping)
- [ ] EnemyBase + EnemyAI state machine
- [ ] CombatSystem (attack resolver, hit states, energy)
- [ ] TraitAccumulator + EvolutionChecker
- [ ] ShrineController
- [ ] DayNightCycle
- [ ] SaveManager (JSON + Keychain layers)
- [ ] AudioManager skeleton
- [ ] HUD skeleton (energy bar, health)

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
