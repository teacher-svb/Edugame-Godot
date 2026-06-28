---
id: "level-wizard-school-2026-06-28"
status: "todo"
priority: "high"
assignee: null
epic: "world"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["level", "act-1", "world"]
order: "lvl-9"
---
# Level: Wizard School (trial chambers)

**Used by:** I.3, V.3 | **Acts:** I, V

Three elemental chambers: Fire, Water, Earth. Each is a distinct room accessed sequentially. The L1 logic challenge is the chamber itself — traversal puzzle with hazards. The Spark Test platform is at the far end of each chamber.

**Key interactive elements:**
- Fire chamber: flame jet hazard columns, Spark Test platform
- Water chamber: moving platforms over water, Spark Test platform
- Earth chamber: falling rocks / shifting ground, Spark Test platform
- Entry hall (connects all three chambers)

**Build tasks:**
1. Compose Wizard School building exterior + entry hall in Blender.
2. Compose Fire chamber — jets of flame blocking path, platform at end.
3. Compose Water chamber — water gaps with moving platforms.
4. Compose Earth chamber — falling debris / shifting ground, platform at end.
5. Export and import all rooms into Godot scenes (one scene per chamber or sub-scenes).
6. Set up collision, navigation mesh for each chamber.
7. Place elemental wizard NPCs on end platforms.
8. Configure hazard nodes (flames, platforms, debris) as L1 logic puzzle elements.
