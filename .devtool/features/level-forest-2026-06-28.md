---
id: "level-forest-2026-06-28"
status: "backlog"
priority: "medium"
assignee: null
epic: "world"
dueDate: "2026-09-30"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["level", "act-4", "world"]
order: "lvl-10"
---
# Level: The Forest + Mine Clearing

**Used by:** I.4, IV.2 | **Acts:** I (brief), IV

Path between Beaverford and the mine clearing. Ends at a clearing where the mine building stands. Two states:
- **Act I (I.4):** Daytime. Mabel walks home through the forest. Atmospheric/narrative only. Guarded mine entrance visible but inaccessible.
- **Act IV (IV.2):** Total darkness. Navigable only with Mechanical Lantern. Guards absent. L2 logic puzzle — timing and navigation in limited light radius.

**Key interactive elements:**
- Mine building exterior in the clearing (entry point to Mine level)
- Mana Spring entrance gate (guarded in Act I; unguarded/open in IV.2)
- Dark/limited visibility zone (lantern radius mechanic — IV.2 only)

**Build tasks:**
1. Compose forest path in Blender — winding, dense trees, opening into a dead clearing at the far end.
2. Compose clearing: barren earth, nothing grows, mine building visible at centre.
3. Export and import into Godot scene.
4. Set up collision mesh, navigation mesh.
5. Implement day/night lighting state switch.
6. Implement lantern radius for Act IV: only area within lantern range is lit; rest is dark.
7. Configure L2 hazard/obstacle nodes along the dark forest path.
8. Place mine entrance gate node (guarded vs. open state).
9. Exit point into Mine level at the mine building door.
