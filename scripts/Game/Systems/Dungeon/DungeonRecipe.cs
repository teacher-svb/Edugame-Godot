using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// What a chamber contains (card dungeon-pop-7a; option 3: fixed slots plus a weighted pool). Per pass,
    /// gates first: the slots of that pass in order, then seeded draws from the pool by
    /// <see cref="DungeonEncounter.Weight"/> until the pass's budget is used or nothing in the pool fits any
    /// more. Slots count towards the budget; a slot that doesn't fit is skipped (and reported). Same graph,
    /// route, free-floor check and seed → same plan.
    /// </summary>
    [GlobalClass, Tool]
    public partial class DungeonRecipe : Resource
    {
        [Export] public Godot.Collections.Array<EncounterSlot> Slots { get; set; } = new();

        [Export] public Godot.Collections.Array<DungeonEncounter> Pool { get; set; } = new();

        /// <summary>Gates (locks) in total, slots included.</summary>
        [Export(PropertyHint.Range, "0,20,1,or_greater")] public int Gates { get; set; } = 1;

        /// <summary>Fillers (objects) in total, slots included.</summary>
        [Export(PropertyHint.Range, "0,50,1,or_greater")] public int Fillers { get; set; }

        /// <summary>The placements, in the context; <paramref name="skipped"/> gets a line per slot that didn't fit.</summary>
        public EncounterContext Plan(DungeonGraphData graph, DungeonRoute route, Func<Vector3I, bool> isFreeFloor, long seed, List<string> skipped = null)
        {
            var context = new EncounterContext(graph, route, isFreeFloor, seed);
            foreach (var pass in new[] { EncounterPass.Gate, EncounterPass.Filler })
            {
                int budget = pass == EncounterPass.Gate ? Gates : Fillers;
                int used = 0;

                foreach (var slot in Slots.Where(s => s?.Encounter != null && s.Encounter.Pass == pass))
                {
                    if (used >= budget)
                        break;
                    var target = slot.TargetOn(route);
                    if (target != null && slot.Encounter.Plan(context, target.Value))
                        used++;
                    else
                        skipped?.Add($"{slot.Encounter.Id} ({slot.Place}{(slot.Place == SlotPlace.PathStep ? $" {slot.Step}" : "")}): no place on this route");
                }

                var pool = Pool.Where(e => e != null && e.Pass == pass && e.Weight > 0).ToList();
                var rng = context.RandomFor($"pool:{pass}");
                while (used < budget && pool.Count > 0)
                {
                    var drawn = Draw(pool, rng);
                    if (drawn.Plan(context, EncounterTarget.Any))
                        used++;
                    else
                        pool.Remove(drawn);   // nothing frees up during planning, so it won't fit later either
                }
            }
            return context;
        }

        static DungeonEncounter Draw(List<DungeonEncounter> pool, Random rng)
        {
            double pick = rng.NextDouble() * pool.Sum(e => e.Weight);
            foreach (var encounter in pool)
            {
                pick -= encounter.Weight;
                if (pick < 0)
                    return encounter;
            }
            return pool[^1];
        }
    }
}
