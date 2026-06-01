# Project Fenrir — Game Design Document v0.1

> **Status:** Draft  
> **Last Updated:** 2026-06-01 (Session 2 — Narrative, Combat, Social, Save locked)
> **Author:** Project Fenrir Team

---

## 1. High Concept

Project Fenrir is a semi-open-world action RPG where players are randomly assigned an elemental affinity at awakening. Through combat, exploration, decision-making, creature interactions, and hidden behavioral patterns, their element evolves into unique forms that reflect how they played — not what they selected.

> **Core Philosophy:**  
> *Fate determines your beginning.*  
> *Your actions determine your evolution.*  
> *Your skill determines your victory.*

---

## 2. Design Pillars

### Pillar 1 — Skill > Level

Player skill is the primary determinant of combat outcomes.

**Leveling unlocks:**

- New abilities
- New passives
- New build options

**Leveling does NOT provide:**

- Massive stat increases
- Automatic combat superiority

### Pillar 2 — Hidden Evolution

Players never directly choose evolutions. The game observes:

- Combat behavior
- Exploration habits
- Moral decisions
- Creature interactions
- PvP performance

Evolution occurs based on hidden conditions — never manual selection.

### Pillar 3 — Discovery Over Grinding

The most exciting moments should be:

- Discovering a hidden area
- Encountering a rare creature
- Triggering an unknown evolution
- Learning forgotten lore

Not simply reaching a level cap.

### Pillar 4 — PvP As A Rite Of Ascension

PvP is not the primary progression system. It becomes meaningful late-game for:

- Certain evolutions
- Legendary trials
- Prestige achievements

Players primarily progress through PvE.

---

## 3. Genre

| Category | Value |
| --- | --- |
| Primary | Action RPG |
| Secondary | Creature Hunting, Exploration, Character Evolution |
| Inspirations | Infinity Blade, Elden Ring, Monster Hunter, Shadow of Mordor |

---

## 4. World Structure

**Type:** Semi-Open World

### Initial Regions

| Region | Alignment | Creatures |
| --- | --- | --- |
| Ember Forest | Fire | Ash Wolves, Flame Sprites, Ember Stags |
| Tidal Marsh | Water | Mire Serpents, Tide Crawlers, Water Wraiths |
| Stone Expanse | Earth | Stonebacks, Crystal Crawlers, Earth Titans |
| Sky Peaks | Air | Storm Hawks, Wind Drakes, Thunder Giants |

---

## 5. Element System

### Common Elements

Fire · Water · Earth · Air  

- Most players receive these
- Highest growth potential

### Rare Elements

Lightning · Metal · Ice · Nature  

- Stronger starting kits
- Reduced evolutionary flexibility

### Very Rare Elements

Light · Darkness · Shadow  

- Powerful starting abilities
- Difficult evolution requirements

### Supreme Elements

Space · Time · Life · Death  

- Extremely powerful beginnings
- Few evolution paths
- Highest mastery requirements

---

## 6. Progression

### Level

Represents experience. Unlocks active/passive abilities and build slots. Does NOT heavily affect combat power.

### Evolution

Represents identity. Triggered by:

- Hidden behavioral metrics
- World choices
- Creature encounters
- Major accomplishments

### Ascension

Late-game. Requires mastery of self, world, and other players. May include PvP trials.

---

## 7. Combat

### Philosophy

Combat should feel dangerous at every stage. A highly skilled low-level player can defeat a poorly played high-level character.

### Core Actions

- Light Attack
- Heavy Attack
- Dodge
- Block
- Counter
- Elemental Ability
- Ultimate Ability

### Combat Success Factors

1. Timing
2. Positioning
3. Resource Management
4. Enemy Knowledge
5. Element Mastery

---

## 8. Hidden Trait System

Traits are invisible to the player. Examples:

`Mercy` · `Aggression` · `Curiosity` · `Sacrifice` · `Dominance` · `Loyalty` · `Wisdom` · `Exploration`

Traits influence evolution paths. Players never see numerical values.

---

## 9. Evolution Example

**Fire** → *(hidden behavior tracking)* → **Inferno** / **Phoenix Flame** / **Plasma**

Evolution is determined by behavioral pattern, not manual selection.

---

## 10. MVP Scope

> *Build the smallest version that answers one question: Is the evolution system fun?*

| Category | MVP Scope |
| --- | --- |
| Elements | Fire, Water, Earth, Air |
| Regions | Ember Forest only |
| Evolutions | 1 per element |
| Combat | Dodge, Light Attack, Heavy Attack, Ability |
| Enemy Types | 5 creature types |
| Bosses | 1 regional boss |
| Progression | Hidden trait tracking + basic evolution system |
| Multiplayer | ❌ None |
| PvP | ❌ None |
| Rare Elements | ❌ None |
| Open World | ❌ None |

---

## 11. Awakening UX (Locked)

| Decision | Choice |
| --- | --- |
| Setting | Player's origin village |
| Witnesses | Public ceremony — village and crowd watch |
| Rarity reveal | Hinted through world reaction (NPCs react, no UI label) |
| Element in appearance | Eyes and hair visually manifest the element post-awakening |
| Reroll UX | Narrative choice: "Accept your fate" / "Challenge fate" |

---

## 12. Combat Design (Locked)

| Decision | Choice |
| --- | --- |
| Camera | Third-person over-the-shoulder |
| Movement | Hybrid — virtual joystick + swipe abilities |
| Ability system | Resource-gated (energy bar builds through combat actions) |
| Death consequence | Lose currency/progress (no items) + death shifts trait tracking |

---

## 13. World Travel (Locked)

Open exploration. Regions are gated by:

- **Boss barriers** — a powerful creature blocks passage (skill gate)
- **Elemental barriers** — require a superior element type or a sufficiently evolved elemental ability (element gate)

This gives rare/supreme elements real mechanical meaning from day one.

---

## 14. Narrative (Locked)

See `LORE.md` for full world lore. Summary:

- The world is built on an elemental hierarchy — Supreme elements rule, Common elements serve
- Fenrir was a legendary figure who proved a Common element could surpass all Supremes through evolution alone
- The War of Principles split the world: **The Ascendants** (hierarchy) vs **The Fenrir Doctrine** (evolution)
- Present day: centuries after Fenrir vanished, strange events are occurring — unknown evolutions, awakening shrines, emerging creatures
- The player is **not the Chosen One** — they begin as an ordinary Awakening and unknowingly walk Fenrir's path
- **Central mystery:** What was Fenrir's final evolution?

**Canonical lines (locked, verbatim):**
> *"The weak worship power. The powerful worship destiny. But destiny is merely the first chain. Break it."*
> *"Fate determines your beginning. Choice determines your evolution. Mastery determines your destiny."*

---

## 15. Social Layer (Locked)

| Feature | Decision |
| --- | --- |
| Element rarity showcase | Rare/Supreme elements visible to others in shared spaces |
| Evolution showcase | New evolution forms are visible to other players when achieved |
| Ghost data / leaderboards | Deferred — not in MVP |
| Full multiplayer | Deferred — backend required |

---

## 16. Save System (Locked)

| Layer | Decision |
| --- | --- |
| Primary | Local-first persistence (on-device) |
| Backup | Optional iCloud sync |
| Backend | Deferred — required for PvP and global discovery systems |

Slot seeds (Awakening) stored in iOS Keychain. See `AWAKENING_SYSTEM.md`.

---

## 17. Open Design Questions

- [x] **Awakening System** — See `AWAKENING_SYSTEM.md`
- [x] **Camera** — Third-person over-the-shoulder
- [x] **Combat movement** — Joystick + swipe hybrid
- [x] **Ability system** — Resource-gated energy bar
- [x] **Death system** — Currency loss + contextual trait shift (see `HIDDEN_TRAIT_SYSTEM.md`)
- [x] **World travel** — Open exploration, boss/elemental barriers
- [x] **Narrative** — See `LORE.md`
- [x] **Save system** — Local-first + optional iCloud
- [x] **Trait visibility** — Vague journal entry appears after first evolution only. No UI, no numbers.
- [x] **Slot 2 unlock** — Available at account creation. Both slots open from day one.
- [x] **Character creation scope** — Name + gender + face preset + body type. Element manifests in eyes/hair post-awakening.
- [x] **Fenrir Doctrine in MVP** — Hidden shrine with cryptic lore fragment in Ember Forest. No NPC. Found through exploration.
- [x] **Death trait specifics** — Contextual by death type. See `HIDDEN_TRAIT_SYSTEM.md` Combat Signals table.

---

*Next design section: **The Awakening System***
