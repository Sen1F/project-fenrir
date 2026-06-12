# Changelog

All notable changes to Project Fenrir will be documented here.
Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [Unreleased]

### Added

- Phase 2 architecture (PR #3/#4): AppStateMachine, SceneRouter, GameLoop,
  CombatContext, death classification (5 types), DeathMessageProvider,
  EventLogger verbose trait logging, `Fenrir → Setup Project` one-command
  editor setup, Ash Wolf first enemy, NavMesh bake, URP auto-repair
- Engineering plan (`PLAN.md`) with ticketed phases P2–P5

### Changed

- Unity 6 LTS (6000.4.9f1) upgrade; Input System Enhanced Touch
- All timestamps Unix seconds (`long`) — JsonUtility cannot serialize DateTime
- BehaviorEventBus cleared by SceneRouter before scene activation (was
  GameLoop after completion — wiped new-scene subscriptions)
- AudioManager static singleton replaced with ServiceLocator registration

### Removed

- Legacy duplicate source trees `src/Assets/`, `src/Tests/` (P2-01)
- Unity CI build job — Personal license cannot activate headless

### Fixed

- EventLogger handlers lost after scene transitions
- Enemy spawn inside detection range; NavMeshAgent pushing player
  (trigger collider)

## [0.1.0] — Phase 0/1

- Game Design Document v0.1
- World Lore v0.1 (The Fenrir Principle)
- Awakening System Design v0.1
