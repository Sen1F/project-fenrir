# Project Fenrir — World Design v0.1 (Ember Forest)

> **Status:** Draft  
> **Last Updated:** 2026-06-01  
> **Scope:** MVP region only — Ember Forest  
> **Depends on:** GDD.md §4 (World Structure), COMBAT_SYSTEM.md, HIDDEN_TRAIT_SYSTEM.md

---

## Design Goals for Ember Forest

1. Teach every core mechanic without a tutorial screen
2. Contain enough behavioral variety to push traits in all 10 directions
3. House the evolution system's first shrine trigger
4. End with a boss that validates everything the player has learned
5. Leave the player wanting to find what's past the elemental barrier

---

## Region Overview

**Biome:** Ash woodland — a forest scorched long ago, slowly regenerating. Ember-lit at dusk. Ash-grey at dawn. The air smells of old fire.

**Tone:** Dangerous but navigable. Not oppressive. The danger feels earned, not imposed.

**Size:** Designed for 8–15 hours of focused play (targeting first evolution within this window).

**Elemental alignment:** Fire — creatures here are Fire-attuned or Fire-resistant. Non-fire players are at a slight disadvantage. Fire players feel native.

---

## Sub-Zones

### 1. The Threshold

*Where the player enters after the Awakening ceremony.*

A wide, open clearing. Ash Wolves roam in small packs of 2. No hazards. Long sight lines. This is where the player learns basic movement and light attack — not through a prompt, but through necessity.

**Points of interest:**
- **The Village Edge** — the origin village is visible in the distance (Loyalty signal for players who linger and look back)
- **First Waystone** — immediately accessible; teaches checkpoint system implicitly
- **Dead tree with scratch marks** — lore object: claw marks too large for any known creature (Curiosity signal if examined)

**Creatures:** Ash Wolves (2–3 packs), Flame Sprites (2 isolated)

---

### 2. The Ashwood

*Deep forest. Tighter paths, more cover, more ambush opportunity.*

The main traversal zone. Players pass through here multiple times. Sub-paths reward exploration. Ash Revenant enemies here teach the guard-break mechanic.

**Points of interest:**
- **Hidden hollow** — accessible only by leaving the main path; contains a rare lore fragment and a Curiosity/Exploration signal
- **Abandoned camp** — a dead fire with scattered items; suggests others came before; Wisdom signal if the player reads the camp carefully (lore object)
- **Flame Sprite nest** — a cluster of 5–6 sprites protecting a resource cache; rewards aggressive clearing

**Creatures:** Ash Wolves, Flame Sprites, Ash Revenants

**Trait signals concentrated here:** Curiosity (exploration), Wisdom (lore), Dominance (Revenant combat)

---

### 3. The Ember Fields

*An open, ember-lit meadow. Ember Stags graze here.*

The player's first encounter with a creature that is not purely hostile. Ember Stags are passive until provoked. Players who attack them increment Mercy↓ and Aggression↑. Players who walk among them without attacking increment Curiosity↑ and Mercy↑.

**Points of interest:**
- **The Scorched Circle** — a perfect ring of ash with no explanation; lore object; Wisdom signal; later revealed to be the site of an ancient elemental confrontation
- **Waystone 2** — located here
- **Hidden path** — a trail behind a cluster of rocks leads to the Doctrine Shrine (see §Doctrine Shrine)

**Creatures:** Ember Stags (passive), Ash Wolves (2 packs at perimeter), Inferno Wisps (3, nocturnal — appear after the player has rested at a waystone)

**Trait signals concentrated here:** Mercy (creature interaction), Curiosity (scorched circle, hidden path), Exploration (finding the hidden path)

---

### 4. The Doctrine Shrine

*Hidden. Requires leaving the main path in the Ember Fields and navigating a narrow passage.*

A stone shrine unlike any other in the forest — older, deliberately maintained despite the surrounding decay. It is not an Evolution Shrine. It is a record shrine. A hidden archive left by the Fenrir Doctrine.

**Contents:**
- A lore fragment, partially legible:

> *"...the element does not choose the worthy. It becomes what you make it. This is what they erased. This is what we preserve. If you are reading this, you found us before we found you."*

- Beneath the text: a symbol that matches no known faction — it will appear again later in the game in contexts the player will recognise

**Mechanics:**
- No combat encounter here
- Interacting with the lore fragment increments Curiosity↑, Wisdom↑
- The shrine itself is an Evolution Shrine in dormant state (unlocks as active if evolution threshold is met on return)
- Players who find this before their first evolution will have a different reaction to it than those who find it after

---

### 5. The Char Ravine

*A narrow gorge carved by an ancient fire event. High walls, tight quarters, limited dodge space.*

This is the highest-difficulty zone in Ember Forest before the boss. Combat encounters here are designed to punish reckless play — there is no room to reset distance. Players who rely on dodge-heavy play must adapt. Players who rely on blocking are at home.

**Points of interest:**
- **Ravine Shrine** — an Evolution Shrine in stirring or active state depending on player progress; the most likely location for a first evolution given it is encountered after significant play time
- **The Ash Titan's Mark** — a massive handprint burned into the ravine wall; no context given; lore object for Wisdom signal; connects to post-MVP content (Tidal Marsh has a corresponding mark)
- **Hidden alcove** — reachable by a narrow ledge path; contains a rare resource and an Exploration signal

**Creatures:** Ash Revenants (3–4, including one Elite with increased HP and a second attack pattern), Inferno Wisps, Ember Stags (2 — blocked path, players must decide whether to fight or find another route)

**Trait signals concentrated here:** Sacrifice (taking hits in tight space), Patience (waiting for gaps in confined combat), Recklessness (rushing through without reading the space)

---

### 6. The Emberlord's Sanctum

*The boss arena. Reached after clearing the Char Ravine.*

A wide circular arena — open enough for movement, enclosed enough that there is no running. The floor is cracked volcanic stone. The air is hot. Flame Sprites circle the perimeter but do not engage unless summoned in Phase 2.

**Pre-boss waystone:** Directly before the arena entrance. Cannot be missed.

**Post-boss:**
- Elemental barrier at the far end of the sanctum deactivates
- A path opens toward the border of Ember Forest
- A brief environmental shift — the ash in the area begins to cool; as if the forest is exhaling
- A lore object appears beside the fallen Emberlord: a fragment that references Fenrir by implication without naming him

---

## Shrine Placement Summary

| Shrine | Location | Default State |
|---|---|---|
| Doctrine Shrine | Hidden path off Ember Fields | Not an evolution shrine — lore only |
| Ravine Shrine | Mid-Char Ravine | Stirring (active if threshold met) |
| Sanctum Shrine | Post-boss in the Emberlord's Sanctum | Active (post-boss; highest-signal location) |

Players who evolve at the Ravine Shrine: traits dominated by combat and creature behavior.  
Players who evolve at the Sanctum Shrine post-boss: the boss kill is the final high-signal event — this is the intended first-time experience.  
A player who finds neither shrine in time will have them available on return.

---

## Exploration Rewards Summary

| Discovery | Location | Trait Signal |
|---|---|---|
| Dead tree with scratch marks | The Threshold | Curiosity↑ |
| Hidden hollow | The Ashwood | Curiosity↑, Exploration↑ |
| Abandoned camp (read carefully) | The Ashwood | Wisdom↑ |
| Scorched Circle | Ember Fields | Wisdom↑, Curiosity↑ |
| Doctrine Shrine fragment | Hidden path, Ember Fields | Curiosity↑, Wisdom↑ |
| Ash Titan's Mark | Char Ravine | Wisdom↑ |
| Hidden alcove | Char Ravine | Exploration↑ |
| Post-boss lore object | Sanctum | Wisdom↑ |
| Watching Ember Stags (no attack) | Ember Fields | Curiosity↑, Mercy↑ |
| Revisiting The Threshold after hours of play | Any return visit | Loyalty↑ |

---

## Open Questions

- [ ] Does the origin village have any interactive NPCs in MVP, or is it purely the ceremony location?
- [ ] Is there a day/night cycle in Ember Forest? (Inferno Wisps suggest nocturnal behaviour — this implies one)
- [ ] Are there any merchant/trader characters in MVP, or is all equipment found in the world?
- [ ] What is the region transition moment — is there a cutscene, a loading screen with a lore line, or a seamless walk-through?
- [ ] Are there any platforming or traversal challenges (ledges, jumps) or is all movement combat-focused?
