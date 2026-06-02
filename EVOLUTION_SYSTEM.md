# Project Fenrir — Evolution System Design v0.1

> **Status:** Draft  
> **Last Updated:** 2026-06-01  
> **Depends on:** HIDDEN_TRAIT_SYSTEM.md, AWAKENING_SYSTEM.md, GDD.md §6

---

## Purpose

Evolution is the identity engine of Project Fenrir. It is the answer to the question the game never stops asking: *what kind of person are you becoming?*

An evolution is not a reward for playing enough. It is a consequence of playing a certain way. Two players with the same element, same level, same hours played will evolve differently — because they are different.

---

## Structure

### Evolution Tiers

```text
Tier 0 — Base Element     (Awakening assignment)
    ↓
Tier 1 — First Evolution  (MVP scope — 1 per element)
    ↓
Tier 2 — Second Evolution (post-MVP — 2–3 per Tier 1 form)
    ↓
Tier 3 — Ascendant Form   (late-game — 1 per Tier 2, PvP/trial gated)
```text
**MVP implements Tier 0 → Tier 1 only.**

### Evolution Paths Are Not Trees — They Are Signatures

Each evolution has a trait signature. The player does not "choose a branch." The trait system observes behavior and when a shrine is activated, the best-fit signature wins. If the player's behavior changes completely after Tier 1, their Tier 2 evolution can be entirely different from what their Tier 1 would predict.

Evolution does not lock a direction. It reflects a moment.

---

## Trait Carryover on Evolution

When a Tier 1 evolution triggers, trait values do **not** fully reset. They shift 30% toward neutral (50.0):

```text
newValue = currentValue + (50.0 - currentValue) × 0.30
```text
This means a player who evolved via extreme Aggression (value: 85) enters Tier 1 with Aggression at ~75 — still dominant, but no longer maxed. The next evolution requires them to either maintain and deepen that pattern, or genuinely change direction.

**Why 30% and not 100%?** A full reset would mean early behavior is irrelevant. Zero reset would mean Tier 1 evolution completely predetermines Tier 2. 30% preserves identity continuity while allowing genuine change.

---

## MVP Evolutions — Tier 1

### Fire → Inferno

**Trait Signature:**

```text
Aggression   ≥ 70
Dominance    ≥ 65
Mercy        ≤ 35
Recklessness ≥ 55
```text
**Profile:** A fighter who attacks relentlessly, controls every engagement, shows no mercy to enemies or creatures, and accepts damage as a cost of dominance.

**Mechanical Changes:**

- *Elemental Ability* replaces base Fire Surge: **Inferno Spiral** — a spinning AoE burst that deals continuous burn damage in a radius; costs full energy bar
- *New Passive* — **Berserker's Ember**: attack speed increases by 15% when below 40% HP
- *New Ultimate* — **Conflagration**: the player ignites, dealing burn to all nearby enemies for 8 seconds; movement speed +20% during activation
- *Visual*: eyes deepen from amber to deep crimson; hair tips ignite permanently (cosmetic flame effect, not damaging)

**World Reaction:** Upon evolution, Ash Wolves in the area scatter. Flame Sprites that were hostile become passive briefly — as if recognising something.

---

### Water → Abyssal Current

**Trait Signature:**

```text
Patience     ≥ 70
Wisdom       ≥ 65
Curiosity    ≥ 60
Aggression   ≤ 40
```text
**Profile:** A player who reads enemy patterns before engaging, explores thoroughly, waits for the right moment, and avoids unnecessary confrontation.

**Mechanical Changes:**

- *Elemental Ability* replaces base Water Jet: **Undertow** — a targeted pull that drags an enemy into a vulnerable state; costs 60% energy
- *New Passive* — **Still Water**: blocking within 0.5s of an attack restores 15% energy
- *New Ultimate* — **Abyssal Surge**: the player becomes liquid-state for 3 seconds — immune to damage, passes through enemies, exits with a burst wave
- *Visual*: eyes shift to deep teal; hair takes on a permanent wet-dark sheen with faint luminescence

**World Reaction:** Upon evolution, rain begins in the local area for 60 seconds regardless of weather state. Tide Crawlers (Water-aligned creatures) become passive.

---

### Earth → Bedrock

**Trait Signature:**

```text
Sacrifice    ≥ 70
Loyalty      ≥ 65
Dominance    ≥ 55
Exploration  ≤ 40
```text
**Profile:** A player who endures punishment without retreating, completes every quest, revisits known areas, and holds ground rather than seeking new territory.

**Mechanical Changes:**

- *Elemental Ability* replaces base Stone Slam: **Fault Line** — a ground-crack that stuns all enemies in a line; costs 70% energy
- *New Passive* — **Unbroken**: damage taken below 25% HP is reduced by 25%
- *New Ultimate* — **Petrify**: the player crystallises for 4 seconds, becoming immovable but immune; upon exit, the crystal shatters outward dealing area damage
- *Visual*: eyes turn granite-grey with faint mineral speckling; skin develops subtle stone-vein patterns at the temples

**World Reaction:** Upon evolution, the ground trembles briefly. Stonebacks (Earth-aligned creatures) stop moving and face the player — then turn away slowly.

---

### Air → Galeform

**Trait Signature:**

```text
Exploration  ≥ 70
Curiosity    ≥ 65
Patience     ≤ 40
Recklessness ≥ 50
```text
**Profile:** A player who maps every corner, discovers every secret, acts fast without waiting, and moves through the world in quick unpredictable bursts.

**Mechanical Changes:**

- *Elemental Ability* replaces base Wind Dash: **Vortex Step** — a multi-directional dash that leaves a wind trail damaging anything that crosses it for 3 seconds; costs 50% energy
- *New Passive* — **Slipstream**: dodge distance increases by 30%; dodge cooldown reduces by 0.3s
- *New Ultimate* — **Eye of the Gale**: the player enters a wind-state that grants 3 free dodges with no cooldown for 5 seconds
- *Visual*: eyes become pale silver-white; hair lifts and moves as if in a constant breeze

**World Reaction:** Upon evolution, a wind surge sweeps the area. Storm Hawks in the region circle the player once, then fly away.

---

## Evolution Sequence — The Violent Takeover

When a player activates a shrine with a fully eligible trait signature, the following sequence plays. It cannot be skipped.

**Phase 1 — Resistance (0–3s)**
The player loses control of their character. The character staggers. The element begins forcing its way through — fire spreads across skin, water ripples distort the air, stone cracks through the ground, wind tears at clothing. The player watches, cannot act.

**Phase 2 — Break (3–6s)**
The character collapses to one knee. The element erupts — a burst of elemental energy outward. Any nearby enemies are physically pushed back. The player's appearance begins changing.

**Phase 3 — Stillness (6–8s)**
Everything goes quiet. The character stands. Eyes have changed. The world feels different — not because it is, but because the player is.

**Phase 4 — Resume (8s)**
Player regains control. New abilities are available immediately. No tutorial popup. No explanation. The player discovers what changed by trying.

**Post-evolution — Journal Entry (triggered on next pause/menu open)**
A single vague line appears in the journal. It is written as the character's internal voice, not a system notification. Example entries:

> *"The fire feels different. Less like something I carry. More like something that knows me."* — Inferno
> *"I noticed something in the water today. It moved before I did."* — Abyssal Current

> *"I don't move the same way anymore. The ground doesn't resist me. It remembers."* — Bedrock
> *"I reached the other side before I decided to."* — Galeform

---

## Post-MVP Evolution Paths (Planned, Not Designed)

These are placeholder names only — not designed. Listed to prevent future naming conflicts and to confirm design intent.

### Fire Tier 1 Variants (post-MVP)

- **Inferno** — Aggression/Dominance/Recklessness (MVP)
- **Phoenix Flame** — Sacrifice/Mercy/Exploration
- **Plasma** — Curiosity/Wisdom/Aggression

### Water Tier 1 Variants

- **Abyssal Current** — Patience/Wisdom/Curiosity (MVP)
- **Tidal Wrath** — Aggression/Dominance/Recklessness
- **Mirror Surface** — Mercy/Loyalty/Sacrifice

### Earth Tier 1 Variants

- **Bedrock** — Sacrifice/Loyalty/Dominance (MVP)
- **Verdant Growth** — Mercy/Curiosity/Exploration
- **Seismic** — Aggression/Recklessness/Dominance

### Air Tier 1 Variants

- **Galeform** — Exploration/Curiosity/Recklessness (MVP)
- **Tempest** — Aggression/Dominance/Sacrifice
- **Whisper** — Patience/Wisdom/Mercy

---

## Evolution and Region Access

Evolution form affects world access beyond the base element:

- **Inferno** can melt Ice-class elemental barriers (post-MVP regions)
- **Abyssal Current** can navigate underwater zones inaccessible to base Water
- **Bedrock** can break through Stone-class barriers base Earth cannot
- **Galeform** can reach elevated zones with no climbable path

This creates a layered gate system: element type opens the first door, evolution form opens the second.

---

## Locked Decisions

- [x] **Tier 1 is permanent.** First evolution cannot be changed within a playthrough. Identity is fixed at Tier 1. Tier 2 branches from whatever Tier 1 form was reached.
- [x] **Trait carryover rule applies at every tier.** 30% shift toward neutral on each evolution, consistently.
- [x] **Ascendant reaction post-evolution:** world dialogue shifts — NPCs comment, rumors spread. No agents dispatched in Ember Forest.
- [x] **Reverse evolution:** not possible. Behavioral change redirects Tier 2+ paths, never Tier 1.

## Open Questions

- [ ] Does the evolution form affect the Awakening ceremony on Slot 2?
- [ ] Is there a maximum number of evolutions per playthrough, or is each tier always available?
