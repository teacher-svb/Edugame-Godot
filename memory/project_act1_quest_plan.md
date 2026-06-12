---
name: project-act1-quest-plan
description: Act I quest and level plan — what needs to be built, waiting on level design to finish first
metadata:
  type: project
---

Act I quest structure is designed, waiting on level completion before implementing.

**Why:** Quests reference specific scene nodes/areas, so levels must be laid out before quest .tres files and scene triggers can be wired up.

**How to apply:** When user says levels are done, resume from here.

## Level → Chapter mapping

- **Level1_3D.tscn** — Mabel's house (bedroom, living room, kitchen)
  - I.1: Tutorial quest already exists as `LookingForAvocado.tres`. Last objective should direct Mabel to go talk to people in the village.
- **Level2 (village, scene name unknown/in progress)** — Beaverford village
  - I.1 end: transports here from Level1
  - I.2: New quest — Mabel talks to villagers, tries to help, gets rejected, ends with goal of going to wizard school
- **Wizard school level (not yet designed)** — needs new scene
  - I.3: Three quests, one per elemental wizard (Fire, Water, Earth). Each is a world puzzle (L1) + spark test framed as failure.
  - I.4: Triggered when all 3 wizard quests complete — Mabel is sent home

## Quest files to create (once levels ready)
1. Modify `LookingForAvocado.tres` — add final objective directing player to village
2. New: `TheAmbition.tres` — village quest, I.2
3. New: `WizardTrialFire.tres` — I.3, fire wizard
4. New: `WizardTrialWater.tres` — I.3, water wizard
5. New: `WizardTrialEarth.tres` — I.3, earth wizard

## Characters available
- `Character_022` — Mabel
- `Character_007` — Dad
- `avocado_CharacterData` — Avocado (cat)
- `character_friend1.tscn` — available NPC, no CharacterData file seen yet

## Language
All quest dialogue is in Dutch (existing quest confirms this).
