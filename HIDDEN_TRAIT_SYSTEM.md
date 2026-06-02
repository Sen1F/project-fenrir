# Project Fenrir — Hidden Trait System Design v0.1

> **Status:** Draft  
> **Last Updated:** 2026-06-01  
> **Depends on:** GDD.md §8 (Hidden Trait System), AWAKENING_SYSTEM.md

---

## Purpose

The Hidden Trait System is the engine behind the entire evolution mechanic. It observes player behavior across every domain — combat, exploration, decisions, economics — translates those behaviors into weighted trait values, and determines when an evolution becomes available.

The player never sees any of this. No meters. No hints. No percentages.

The world reacts. The element changes. The player figures it out.

---

## Design Constraints

- **No direct player visibility** — trait values are never exposed in any UI
- **All inputs are passive** — traits accumulate as a side effect of normal play, never through dedicated "trait actions"
- **No single behavior dominates** — a player cannot grind one action repeatedly to force an evolution
- **Multiple valid paths** — for each element, multiple evolutions must be reachable through genuinely different playstyles
- **Decay exists** — trait values shift over time based on continued behavior, not just early choices

---

## Trait Definitions

Ten traits are tracked per character. Each value is a float in **[0.0, 100.0]**. All start at 50.0 (neutral) — not zero. This prevents early game behavior from disproportionately locking a player's identity.

| Trait | High value means | Low value means |
| --- | --- |
| **Aggression** | Attacks relentlessly, minimal defense | Patient, measured, defensive |
| **Mercy** | Spares creatures, avoids unnecessary kills | Ruthless, efficient, eliminates threats |
| **Curiosity** | Explores, experiments, seeks unknown | Focused, linear, goal-oriented |
| **Sacrifice** | Accepts damage/cost to protect or achieve | Self-preserving, efficient resource management |
| **Dominance** | Controls engagements, counters, executes | Reactive, adaptive, evasive |
| **Loyalty** | Returns to places/NPCs, completes quests | Transient, moves on, rarely revisits |
| **Wisdom** | Reads the world, learns enemy patterns, plans | Reactive, improvises, ignores lore |
| **Exploration** | Seeks every corner, finds secrets | Direct paths, skips optional content |
| **Recklessness** | Dies frequently, takes unnecessary risks | Careful, methodical, rarely dies |
| **Patience** | Waits, observes, charges abilities fully | Impatient, rapid-fires, interrupts |

---

## Signal → Trait Mapping

### Combat Signals

| Behavior | Trait(s) Modified |
| --- | --- |
| Dodge used (per combat) | Patience ↑, Aggression ↓ |
| Dodge never used (combat completed) | Aggression ↑, Recklessness ↑ |
| Counter landed | Dominance ↑, Wisdom ↑ |
| Heavy attack ratio > 70% of attacks | Aggression ↑ |
| Light attack ratio > 70% of attacks | Patience ↑, Wisdom ↑ |
| Ability used at full energy | Patience ↑, Wisdom ↑ |
| Ability used below 30% energy | Recklessness ↑ |
| Blocked an attack | Patience ↑, Sacrifice ↑ |
| Took hit without blocking/dodging | Recklessness ↑, Sacrifice ↑ |
| Defeated enemy at low health (high risk) | Dominance ↑ |
| Fled combat | Recklessness ↓, Mercy ↑ |
| **Reckless death** (no dodge/block used) | Recklessness ↑, Patience ↓ |
| **Sacrifice death** (died while still attacking at low HP) | Sacrifice ↑, Recklessness ↑ (slight) |
| **Overwhelmed death** (3+ enemies) | Recklessness ↓ (not culpable), Wisdom ↓ (slight) |
| **Pattern failure** (died to same enemy type 3+ times) | Wisdom ↓ |
| **Ambush death** (died within 5s of entering new area) | No trait shift |
| 10-combat kill streak without dying | Dominance ↑, Wisdom ↑ |

### Creature Signals

| Behavior | Trait(s) Modified |
| --- | --- |
| Spare a creature (disengage at low HP) | Mercy ↑, Curiosity ↑ |
| Kill every creature in an area | Mercy ↓, Dominance ↑ |
| Observe creature without attacking (linger >10s) | Curiosity ↑, Wisdom ↑ |
| Interact with non-hostile creature | Curiosity ↑, Loyalty ↑ |
| Hunt same creature type repeatedly | Dominance ↑, Aggression ↑ |
| Discover a rare/hidden creature | Exploration ↑, Curiosity ↑ |
| Kill boss without taking damage | Dominance ↑, Wisdom ↑, Recklessness ↓ |

### World / NPC Signals

| Behavior | Trait(s) Modified |
| --- | --- |
| Choose mercy in dialogue | Mercy ↑, Loyalty ↑ |
| Choose punishment in dialogue | Mercy ↓, Dominance ↑ |
| Help an NPC (side quest completion) | Loyalty ↑, Wisdom ↑ |
| Ignore NPC request | Loyalty ↓ |
| Read lore object / inscription | Wisdom ↑, Curiosity ↑ |
| Return to origin village | Loyalty ↑ |
| Pay to access a shortcut | Patience ↓, Wisdom ↑ |
| Discover a secret area | Exploration ↑, Curiosity ↑ |
| Revisit a cleared area | Loyalty ↑, Curiosity ↑ |

### Economic Signals

| Behavior | Trait(s) Modified |
| --- | --- |
| Spend primarily on offensive upgrades | Aggression ↑ |
| Spend primarily on defensive upgrades | Sacrifice ↑, Patience ↑ |
| Spend on lore / knowledge items | Wisdom ↑, Curiosity ↑ |
| Hoard currency (large unspent pool) | Patience ↑, Wisdom ↑ |
| Spend immediately upon earning | Recklessness ↑ |

### Exploration / Area Signals

| Behavior | Trait(s) Modified |
| --- | --- |
| % of region map revealed > 80% | Exploration ↑, Curiosity ↑ |
| Take direct path to objective | Exploration ↓, Patience ↓ |
| Spend extended time in dangerous zones | Aggression ↑, Recklessness ↑ |
| Spend extended time in hub / safe zones | Loyalty ↑, Patience ↑ |
| Find all secrets in a zone | Exploration ↑, Wisdom ↑ |

---

## Weighting and Accumulation

### Delta Model

Trait values do not jump by fixed amounts. Each signal applies a **weighted delta** based on context:

```text
newValue = clamp(currentValue + (delta × weight × recencyMultiplier), 0.0, 100.0)
```text
**Weight factors:**

- **Event significance** — killing a boss contributes more than a routine kill
- **Frequency dampening** — repeated identical actions have diminishing returns (prevents grinding)
- **Recency multiplier** — recent behavior carries more weight than old behavior (rolling window)

### Frequency Dampening

For any repeatable action, the delta halves after the 5th occurrence in a session and again after the 10th. This prevents a player from spamming dodge or spare actions to force a trait value.

```text
effectiveDelta = baseDelta × (0.5 ^ max(0, occurrenceCount - 5) / 5)
```text
### Decay

Traits decay toward 50.0 (neutral) over real time when not actively played. Decay rate is slow — not enough to punish short sessions, but enough that a player who completely changes their style over weeks will see their traits shift.

```text
decayedValue = currentValue + (50.0 - currentValue) × decayRate × daysSinceLastPlay
decayRate = 0.02 (2% drift toward neutral per day of inactivity)
```text
**MVP note:** Disable decay for MVP. Implement accumulation-only for the prototype. Decay adds tuning complexity before the core loop is validated.

---

## Evolution Eligibility

Each evolution requires a **trait signature** — a set of traits that must meet threshold conditions simultaneously. Signatures are ranges, not exact values.

### Example: Fire Element Evolutions (MVP)

#### Inferno

```text
Aggression   ≥ 70
Dominance    ≥ 65
Mercy        ≤ 35
Recklessness ≥ 55
```text
*Profile: A relentless fighter who dominates engagements and shows no mercy.*

**Phoenix Flame** *(post-MVP)*

```text
Sacrifice    ≥ 70
Mercy        ≥ 60
Exploration  ≥ 65
Loyalty      ≥ 55
```text
*Profile: Someone who endures hardship, protects others, and wanders widely.*

**Plasma** *(post-MVP)*

```text
Curiosity    ≥ 70
Wisdom       ≥ 65
Aggression   ≥ 55
Patience     ≤ 40
```text
*Profile: A fast, intelligent fighter who learns systems and exploits them.*

### Signature Resolution

If multiple evolution signatures are simultaneously satisfied:

1. Calculate a **fit score** for each: sum of how far each trait exceeds (or falls below) its threshold
2. The evolution with the highest fit score wins when the shrine is triggered
3. If two evolutions are within 10 points of each other: the player's **most recent dominant behavior** breaks the tie

This ensures the player's current identity wins, not just their cumulative history.

---

## The Shrine Trigger

Evolution does not happen automatically when thresholds are met. The player must visit an **Evolution Shrine**.

### Shrine Behavior States

| State | Condition | Visual |
| --- | --- |
| **Dormant** | No evolution eligible | Dark, inert stone |
| **Stirring** | Any evolution >80% of threshold | Faint elemental glow — matches player's element |
| **Active** | Any evolution fully eligible | Bright pulse, audible hum |

Players will notice shrines stirring before they are eligible. This creates anticipation without revealing which evolution is coming or what the conditions are.

### Shrine Interaction

When a player interacts with an **Active** shrine:

1. The shrine reads the current TraitProfile
2. It resolves which evolution signature has the highest fit score
3. The evolution sequence triggers — violent and involuntary
4. The element transforms; new abilities unlock; the world reacts

The player receives no explanation of what caused the evolution. The shift simply happens.

### Shrine Placement (Ember Forest — MVP)

- **One shrine** per major sub-zone (3 in Ember Forest)
- Shrines are visible from a distance but require exploration to reach
- The regional boss area contains the most powerful shrine — activating it post-boss kill carries extra weight (the kill counts as a high-significance event immediately before evolution)

---

## Engineering Architecture

### Data Model

```swift
// TraitKey.swift
enum TraitKey: String, CaseIterable, Codable {
    case aggression, mercy, curiosity, sacrifice
    case dominance, loyalty, wisdom, exploration
    case recklessness, patience
}

// TraitProfile.swift
struct TraitProfile: Codable {
    var traits: [TraitKey: Float]
    var lastUpdated: Date
    var sessionEventCounts: [String: Int]  // for frequency dampening

    static func neutral() -> TraitProfile {
        TraitProfile(
            traits: Dictionary(uniqueKeysWithValues: TraitKey.allCases.map { ($0, 50.0) }),
            lastUpdated: Date(),
            sessionEventCounts: [:]
        )
    }
}
```text
### Event System

```swift
// BehaviorEvent.swift
enum BehaviorEvent {
    // Combat
    case dodgeUsed
    case counterLanded
    case characterDied
    case enemyKilledWithNoHitsTaken
    case abilityUsedAtFullEnergy
    case abilityUsedBelowThirtyPercent
    // Creature
    case creatureSpared
    case creatureObserved(duration: TimeInterval)
    case rareCretureDiscovered
    // World
    case dialogueMercyChosen
    case dialoguePunishmentChosen
    case loreObjectRead
    case secretAreaDiscovered
    // Economic
    case offensiveUpgradePurchased
    case defensiveUpgradePurchased
    case knowledgeItemPurchased
    // ... etc
}

// TraitAccumulator.swift
final class TraitAccumulator {
    func process(event: BehaviorEvent, profile: inout TraitProfile)
    func applyDecay(to profile: inout TraitProfile, daysSinceLastPlay: Double)
    func checkEvolutionEligibility(profile: TraitProfile, element: Element) -> [EvolutionCandidate]
}
```text
### Separation of Concerns

```text
GameScene  →  emits BehaviorEvent  →  TraitAccumulator  →  updates TraitProfile
                                                        ↓
                                              EvolutionEligibilityChecker
                                                        ↓
                                              ShrineStateManager  →  updates shrine visuals
```text
Game logic emits events. It never reads trait values. The accumulator owns all trait math. This keeps game systems clean and makes the trait engine independently testable.

### Persistence

TraitProfile is serialised as part of the character save file (JSON, not Keychain). It is character-level data — if the character is deleted, the TraitProfile is deleted with it. The new character starts fresh at neutral (50.0 across all traits).

---

## Tuning Strategy

The specific threshold values (e.g. Aggression ≥ 70 for Inferno) and delta weights are **tuning variables**, not hardcoded constants. They live in a config file:

```text
/Config/TraitWeights.json
/Config/EvolutionSignatures.json
```text
This allows threshold tuning without a code change. In Phase 4 (Polish), run playtesting sessions with logging enabled to observe real trait distributions and adjust thresholds so evolutions trigger at the right pace — not too early (trivialises the system), not so late that players never see one.

**Target:** A focused player should see their first evolution between 8–15 hours of play in a single region.

---

## Trait Visibility (Locked)

After a character's **first evolution**, a vague journal entry appears. It does not name traits or explain the system. It reads as an in-world observation — something the character notices about themselves. Example:

> *"Something changed in the fire today. It felt like it recognised me. Like it had been watching."*

No further hints are given. No numerical values. No system explanation. The journal entry is the only acknowledgment that behavior influenced the outcome.

---

## Locked Decisions

- [x] **Trait carryover:** 30% shift toward neutral on every evolution, at every tier. Consistent rule.
- [x] **Evolution lock:** Tier 1 is permanent. No dead ends — every Tier 1 form has Tier 2 paths. But Tier 1 cannot be changed within a playthrough.
- [x] **Post-evolution NPC and creature reactions:** Both react. NPCs comment and rumors spread (Ascendant network notices). Creatures behave differently — the evolved form is recognisable to them in ways the player doesn't yet understand. This is a lore hint: creatures have memory of elemental forms that predates the current civilisation.
- [x] **Fenrir Doctrine and shrine locations:** The Doctrine built the evolution shrines. This is not stated explicitly to the player but is discoverable through lore fragments. The Doctrine maintains them. The Ascendants have been trying to find and suppress them for centuries — which is why shrines are hidden, worn, and deliberately difficult to locate.

## Open Questions

- [ ] Is there a maximum number of evolutions per playthrough (cap at Tier 3), or can each tier continue indefinitely?
- [ ] Do night-time exploration actions increment Curiosity at a higher rate than daytime?

---

*Next design section: **Evolution System** — the full evolution tree, per-element paths, and what changes mechanically upon evolution*
