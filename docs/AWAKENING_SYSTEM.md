# Project Fenrir — Awakening System Design v0.1

> **Status:** Locked  
> **Last Updated:** 2026-06-01  
> **Depends on:** GDD.md §5 (Element System), §6 (Progression)

---

## Overview

The Awakening is the moment a player receives their elemental affinity. It is the most identity-defining event in the game. Every downstream system — evolution, trait tracking, combat, region access — branches from this single assignment.

---

## Core Rules (Locked)

| Rule | Decision |
| --- | --- |
| Binding | Character-bound |
| Characters per account | 2 maximum |
| Reroll | One per character lifetime |
| Reroll cost | Forfeit current element permanently |
| Delete + recreate | Returns to original slot element (pre-reroll) |
| Reroll after deletion | Reroll is consumed permanently — does not reset |

---

## Character Slot Architecture

Each account has exactly **2 character slots**. At account creation, a deterministic element seed is generated for each slot and stored permanently — it never changes, even if the character is deleted.

```text
Account
├── Slot 1
│   ├── slot_seed         (generated once at account creation, immutable)
│   ├── slot_element      (derived from slot_seed, the "fate" element)
│   ├── current_element   (slot_element unless reroll was used)
│   ├── reroll_used       (bool, persists through deletion — never resets)
│   └── character         (null if deleted)
└── Slot 2
    └── (same structure)
```

### Key invariant

`slot_seed` and `slot_element` are **account-level data**, not character-level. A character deletion clears the character record but never touches the slot seed. Recreation always restores `slot_element` as `current_element`.

---

## Rarity Distribution

| Tier | Elements | Total Weight | Per Element | Approx. 1-in |
| --- | --- | --- | --- | --- |
| Common | Fire, Water, Earth, Air | 93.0% | 23.25% | 4 |
| Rare | Lightning, Metal, Ice, Nature | 6.0% | 1.5% | 67 |
| Very Rare | Light, Darkness, Shadow | 0.9% | 0.30% | 333 |
| Supreme | Space, Time, Life, Death | 0.1% | 0.025% | 4,000 |

Supreme odds are intentionally equivalent to pulling a specific elite icon in a premium EAFC pack — rare enough to be a social event, common enough to exist in a meaningful playerbase.

### MVP note

MVP only implements Common elements. Rare/Very Rare/Supreme weights are defined now so the seed system is built correctly from day one, but non-Common assignments fall back to Common during MVP. The full distribution activates in a future release.

---

## Seed Generation (Engineering Spec)

### Requirements

- Deterministic: same seed always produces the same element
- Tamper-resistant: cannot be manipulated via save editing or reinstall
- Offline-capable for MVP: no server dependency at launch

### Implementation

```swift
// Pseudocode — full implementation in GameSeedService.swift

struct AccountSeed {
    let slotSeeds: [Int: UUID]  // [slotIndex: seed]
}

func generateSlotSeed() -> UUID {
    UUID()  // Cryptographically random, generated once
}

func deriveElement(from seed: UUID, using distribution: ElementDistribution) -> Element {
    // Use seed bytes to produce a stable float in [0, 1)
    // Walk the weighted distribution table
    // Return the corresponding element
}
```

**Storage:** Slot seeds are written to the iOS Keychain (not UserDefaults, not iCloud) on first account creation. Keychain survives app deletion on device. This prevents:

- Save-scumming via reinstall
- Reroll farming via delete-reinstall loop

**MVP caveat:** If Keychain entry is missing (fresh device, restored from backup without Keychain), generate a new seed and treat it as a first-time account. This is acceptable for MVP — address with server-side account persistence post-launch.

---

## Reroll Rules

1. Each character slot has **one reroll, ever** — `reroll_used` is slot-level, not character-level
2. Using a reroll permanently forfeits the current element
3. The new element is rolled fresh from the full distribution (excluding current element)
4. Deletion does not restore the reroll — if `reroll_used = true`, it stays true
5. The reroll is surfaced to the player as a narrative choice, not a UI button (see UX section)

### Why reroll doesn't reset on deletion

If reroll reset on deletion, a player could delete → recreate → reroll → delete → recreate → reroll indefinitely, effectively getting unlimited rolls while always returning to their fate element. Tying the reroll to the slot permanently closes this loop.

---

## Awakening UX (Ceremony Sequence)

The element reveal is a cinematic scripted sequence — not a UI screen.

### Sequence outline

1. **Arrival** — Player character arrives at the Awakening Site (tutorial end)
2. **Invocation** — World reacts: wind, embers, water, stone all stir
3. **Focus** — The energy narrows to one element, building tension
4. **Reveal** — Element manifests with element-specific VFX and audio
5. **Lock** — Player confirms ("Accept your fate" / "Challenge fate")
   - Accept → character creation completes, slot element locked
   - Challenge → reroll dialog presented *once*, with cost made clear

### Reroll dialog (if triggered)
>
> *"To challenge fate is to surrender what you were given. There is no return."*
> **[ Surrender it ]** / **[ Keep what I have ]**

Confirming reroll immediately discards current element and draws a new one. The new element plays the full reveal sequence again. No further rerolls are offered.

---

## Open Questions (Not Blocking MVP)

- [ ] Does Slot 2 unlock at a specific milestone, or is it available at account creation?
- [ ] Are Supreme elements excluded from MVP reroll outcomes, or can a reroll land on one?
- [ ] Should players ever see their `slot_element` (fate element) after a reroll, or is it hidden?

---

## Relationship to Other Systems

| System | Dependency |
| --- | --- |
| Hidden Trait System | Traits are tracked per character, not per element — evolution paths are element-gated |
| Evolution System | `current_element` determines which evolution tree is active |
| Region Access | Starting region is suggested by element, but not locked |
| Save System | `slot_seed`, `slot_element`, `reroll_used` must survive character deletion |

---

*Next design section: **Hidden Trait System** — how traits are tracked, weighted, and translated into evolution triggers*
