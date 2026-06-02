# Project Fenrir — Combat System Design v0.1

> **Status:** Draft  
> **Last Updated:** 2026-06-01  
> **Depends on:** GDD.md §7 (Combat), EVOLUTION_SYSTEM.md

---

## Philosophy

Combat should feel dangerous at every stage of the game. A highly skilled low-level player should be capable of defeating a poorly played high-level character. Level unlocks options — it does not automate victory.

Every combat encounter should be readable, fair, and punishing when ignored.

---

## Input Scheme

**Platform:** iOS — third-person over-the-shoulder camera

| Input | Action |
| --- | --- |
| Left joystick | Move character |
| Tap (right zone) | Light Attack |
| Hold (right zone) | Heavy Attack (charges for 0.5s, releases on lift) |
| Swipe left/right | Dodge in swipe direction |
| Swipe up | Block (hold to maintain) |
| Swipe down | Counter window (active for 0.3s after enemy telegraphs) |
| Ability button (bottom right) | Elemental Ability (energy-gated) |
| Ultimate button (appears when full) | Ultimate Ability |

### Design Rationale

Swipe-based actions (dodge, block, counter) require intentional inputs — they cannot be accidentally triggered during movement. The joystick and swipe zones are spatially separated to prevent cross-input conflicts.

---

## Core Actions

### Light Attack

- Fast, low damage
- Chains into combos (3-hit standard chain)
- Generates 8% energy per hit landed
- Can be interrupted by dodge mid-chain

### Heavy Attack

- Slow startup (0.5s charge), high damage
- Breaks enemy guard if timed correctly
- Generates 15% energy on hit
- Cannot chain — single strike, then recovery

### Dodge

- Directional, based on swipe direction
- 0.6s cooldown after use
- Invincibility frames: first 0.2s of dodge animation
- Generates 5% energy (Abyssal Current passive modifies this)
- Trait signal: each dodge used increments Patience; combat completed without dodging increments Aggression

### Block

- Held input (swipe up, hold)
- Reduces incoming damage by 60%
- Generates 10% energy on successful block
- Perfect Block (block within 0.2s of impact): full damage nullification + 20% energy + staggers enemy
- Trait signal: blocking increments Patience, Sacrifice

### Counter

- Available for 0.3s after enemy telegraphs a heavy attack (visual: enemy glows red)
- Requires swipe down input within the window
- Successful counter: reflects damage, stuns enemy 1.5s, generates 25% energy
- Miss: player takes full damage, no energy gain
- Trait signal: counter landed increments Dominance, Wisdom

### Elemental Ability

- Energy-gated (requires minimum threshold, varies by ability)
- See EVOLUTION_SYSTEM.md for per-evolution ability specs
- Base (Tier 0) abilities before first evolution:

| Element | Base Ability | Energy Cost | Effect |
| --- | --- | --- | --- |
| Fire | Fire Surge | 50% | Projectile burst, knocks back single target |
| Water | Water Jet | 50% | Targeted stream, slows enemy for 2s |
| Earth | Stone Slam | 60% | Ground strike, AoE stun in small radius |
| Air | Wind Dash | 40% | Forward dash through enemy, dealing damage on exit |

### Ultimate Ability

- Unlocked after first evolution (Tier 1)
- Requires full energy bar (100%)
- See EVOLUTION_SYSTEM.md for per-evolution ultimate specs
- Activating ultimate does not interrupt being hit mid-animation — player must use in a safe window

---

## Energy System

Energy is a shared resource pool — the same bar fuels both abilities and ultimates.

| Property | Value |
| --- | --- |
| Max energy | 100 |
| Starting energy (combat begin) | 0 |
| Light attack hit | +8 |
| Heavy attack hit | +15 |
| Dodge used | +5 |
| Successful block | +10 |
| Perfect block | +20 |
| Counter landed | +25 |
| Damage taken (no block) | +3 |
| Out of combat decay | −5/sec after 3s (resets to 0 before next fight) |

Energy does not persist between combat encounters. Each fight starts at 0, forcing players to build through the engagement before using abilities.

**Design note:** Out-of-combat decay prevents players from building energy on weak enemies then using ultimate on a boss immediately. Boss fights should be earned within the encounter.

---

## Hit States

| State | Trigger | Effect | Duration |
| --- | --- | --- | --- |
| **Stagger** | Heavy attack lands | Interrupts enemy action | 0.8s |
| **Stun** | Counter landed / Perfect Block | Enemy frozen | 1.5s |
| **Knockback** | Fire Surge / Fault Line | Enemy displaced | 1.0s |
| **Slow** | Water Jet | Enemy speed −50% | 2.0s |
| **Burn** | Inferno abilities | −HP/sec | 3.0s |
| **Guard Break** | Heavy attack on guarding enemy | Removes enemy block | Instant |
| **Interrupt** | Any hit during enemy charge | Cancels their attack | Instant |

Player can also enter hit states (same system applies to the player from enemy attacks).

---

## Enemy Design Principles

All enemies follow three rules:

1. **Telegraph before striking** — every dangerous attack has a visible tell (animation wind-up, colour change, audio cue) at least 0.4s before impact
2. **Punish aggression** — every enemy has at least one counter to relentless attacking (a guard, a parry, a retaliation)
3. **Teach by surviving** — dying to an enemy should leave the player understanding what went wrong

### MVP Enemy Archetypes (Ember Forest)

**Ash Wolf** *(Pack creature)*

- Attacks in groups of 2–3
- Individual hits are weak; overwhelm is the threat
- Tell: howl before coordinated lunge
- Weakness: AoE abilities, knockback separates the pack
- Counter: guards nothing — pure aggression; dodging creates space

**Flame Sprite** *(Ranged harasser)*

- Stays at distance, fires ember projectiles
- Moves laterally, difficult to corner
- Tell: charges up (glows brighter) before volley
- Weakness: closing distance — sprites have no melee ability
- Counter: blocking reduces projectile damage by 80%

**Ember Stag** *(Heavy charger)*

- Single target, high damage charge
- Slow between attacks — heavily telegraphed
- Tell: lowers head and scrapes ground before charge
- Weakness: counter window is generous (0.5s); perfect counter stuns for 2.5s
- Counter: cannot be staggered by light attacks; only heavy attacks or counters affect it

**Ash Revenant** *(Adaptive guard)*

- Guards until guard is broken; then retaliates
- Mirrors player attack patterns after 3 encounters
- Tell: shifts guard stance before telegraphing attack type
- Weakness: heavy attacks break guard; counters bypass guard entirely
- Counter: light attack spam has no effect — penalises mindless aggression

**Inferno Wisp** *(Evasive burst)*

- Highly mobile, hard to hit
- Deals burst damage and retreats
- Tell: spins before burst attack
- Weakness: AoE abilities catch it mid-movement; slowing effects ground it
- Counter: attempting to chase it is counterproductive — bait it into approaching

---

## Boss Design — The Emberlord (Regional Boss, Ember Forest)

The Emberlord is the gatekeeper of Ember Forest. Defeating it unlocks the elemental barrier to the next region.

### Design Principles

- **Three-phase fight** — each phase introduces a new mechanic
- **No damage sponge** — phases are marked by behavioral shifts, not HP bars
- **Element-responsive** — the Emberlord reacts differently to each of the four starting elements
- **Trait signal opportunity** — the boss fight is a concentrated burst of behavior signals; how the player fights here meaningfully shifts traits

### Phase 1 — The Ash Crown

HP range: 100% → 65%

Emberlord uses a small moveset: heavy charge, ember swipe, and fire breath cone. All telegraphed clearly. Teaching phase — players learn the timing windows.

Mechanic: **Ash Armour** — the Emberlord is coated in ash that reduces all damage by 40%. Must be stripped by landing 3 consecutive hits without taking damage in between.

### Phase 2 — The Burning Reveal

HP range: 65% → 30%

Ash Armour permanently removed. Emberlord gains speed. Adds a new attack: **Ember Storm** — summons a ring of Flame Sprites. Player must decide: kill sprites (fast but splits attention) or ignore sprites and stay on boss (risky, doable with good dodging).

Mechanic: **Enrage Window** — at 50% HP, the Emberlord enters 10 seconds of increased attack speed. Player must survive without trading damage. Counter: this is the highest Sacrifice/Recklessness signal window in the fight.

### Phase 3 — The Final Flame

HP range: 30% → 0%

Emberlord's fire becomes plasma-hot — all flame attacks now apply a Burn status. Player must manage Burn while finishing the fight.

Mechanic: **Last Stand** — at 15% HP, Emberlord uses its only unblockable attack (clearly telegraphed 2s in advance with full-screen audio/visual cue). Player must dodge. If they do: finish the fight. If they don't: take massive damage but the fight continues (no instant death).

**Design note:** The Emberlord should not be a wall. It should be a test. A player who has learned the enemy archetypes in Ember Forest has already seen all the telegraphs and mechanics the Emberlord uses — just combined and accelerated.

---

## Element × Combat Interactions

Elements are not just cosmetic. They affect how combat plays at base level and how the environment responds.

| Element | Passive combat effect (Tier 0) |
| --- | --- |
| Fire | Light attacks deal +10% damage but player takes +10% damage (no natural resistance) |
| Water | Dodge costs −10 energy instead of +5 (refunded, not charged) |
| Earth | Block damage reduction increased to 70% (vs base 60%) |
| Air | Dodge has one extra invincibility frame (0.25s instead of 0.20s) |

These are baseline differentiators — not so large they define the experience, but enough that players feel their element during combat before any evolution.

---

## Death and Respawn

On player death:

1. Currency lost: 15–25% of current held currency (randomised within range)
2. Trait signal fired: death type evaluated (see HIDDEN_TRAIT_SYSTEM.md)
3. Respawn at last activated checkpoint (campfire / waystone)
4. Enemies in the area reset fully
5. No penalty to XP or progression items

**No death screen.** The character collapses, the screen fades to black, and the character wakes at the checkpoint. Sparse UI. No "YOU DIED" moment — the game respects the player enough not to announce it loudly.

---

## Camera System

**Type:** Third-person over-the-shoulder, fixed follow  
**Lock-on:** Available — tap enemy to lock; camera pivots to maintain enemy in frame  
**Lock-on break:** Swipe away from locked target, or target dies  
**Combat zoom:** Camera pulls back slightly (10%) when 3+ enemies are in range to maintain spatial awareness  
**Boss camera:** Fixed distance override during boss encounters — prevents player from running camera into geometry

---

## Elemental Resistance System (Locked)

Full resistance system applies across all regions.

| Attacker element | Resistant enemies | Weak enemies |
| --- | --- | --- |
| Fire | Fire-aligned creatures (−25% damage) | Water-aligned (+25% damage) |
| Water | Water-aligned (−25%) | Fire-aligned (+25%) |
| Earth | Earth-aligned (−25%) | Air-aligned (+25%) |
| Air | Air-aligned (−25%) | Earth-aligned (+25%) |

**MVP note:** Ember Forest is Fire-aligned. Fire players deal −25% to native creatures but face no incoming penalty. Non-Fire players deal normal damage but face elemental barrier disadvantage in later zones.

Resistance affects ability damage only — not light/heavy attack physical damage. This preserves the skill-over-element philosophy while making element matter.

## XP and Levelling System (Locked)

**Action XP** — kills, discoveries, and quests all contribute.

| Action | XP weight |
| --- | --- |
| Enemy kill | Standard |
| Boss kill | 5× standard |
| Secret area discovered | 2× standard |
| Quest completed | 3× standard |
| Lore object read | 0.5× standard |
| Rare creature encountered | 1.5× standard |

XP distribution reinforces the Discovery Over Grinding pillar — a player who explores and completes quests levels at the same pace as one who only farms enemies.

Level count and unlock cadence: to be defined in ARCHITECTURE.md during Phase 2.

## Energy Bar (Locked)

Bar changes colour at thresholds — no numbers, no text labels.

| State | Colour |
| --- | --- |
| 0–ability threshold | Dim (element colour, low saturation) |
| Ability threshold crossed | Bright (element colour, full saturation) |
| 100% (ultimate ready) | Pulsing white overlay on element colour |

## Locked Decisions

- [x] Elemental resistances: full system, ability damage only
- [x] XP: action-based (kills + discoveries + quests)
- [x] Energy bar: colour-threshold, no numbers
- [x] Counter and parry: same input (swipe down within 0.3s window)

## Open Questions

- [ ] Can abilities be interrupted mid-animation by taking damage?
- [ ] How many total levels exist, and what does each unlock?
- [ ] Region transition — cutscene, loading screen with lore line, or seamless?
