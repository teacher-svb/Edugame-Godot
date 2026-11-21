using System;
using System.Collections.Generic;
using System.Linq;

namespace Godoodads.Dungeon.Core;

/// <summary>
/// Picks the locked doors of a layout: a share of its corridors (room graph edges), chosen from the
/// seed, each locks one of its two doors, so it gets the Locked Door tile. A door that several
/// corridors share locks all of them. Doors of Built rooms are never locked (their cells are yours);
/// a corridor between two Built rooms is skipped. Which door and which corridors follow only from the
/// seed and the layout, so Generate repeats them.
/// </summary>
public static class DoorLocks
{
    /// <summary>Fills <see cref="DungeonLayout.LockedDoors"/>; <paramref name="share"/> is clamped to 0..1.</summary>
    public static void Pick(DungeonLayout layout, float share, long seed)
    {
        layout.LockedDoors.Clear();
        int count = (int)Math.Round(Math.Clamp(share, 0f, 1f) * layout.EdgeDoors.Count, MidpointRounding.AwayFromZero);
        if (count == 0)
            return;

        var random = new Random(unchecked((int)(seed ^ (seed >> 32))));
        var order = Enumerable.Range(0, layout.EdgeDoors.Count).OrderBy(_ => random.Next()).ToList();
        int locked = 0;
        foreach (int edge in order)
        {
            if (locked >= count)
                break;
            var (a, b) = layout.EdgeDoors[edge];
            var lockable = new List<int>(2);
            foreach (int door in new[] { a, b })
                if (door >= 0 && !layout.IsBuilt(layout.Doors[door].Room))
                    lockable.Add(door);
            if (lockable.Count == 0)
                continue;
            layout.LockedDoors.Add(lockable[random.Next(lockable.Count)]);
            locked++;
        }
    }
}
