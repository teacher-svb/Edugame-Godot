using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Where a <see cref="EncounterSlot"/> puts its encounter on the route.</summary>
    public enum SlotPlace { Anywhere, PathStep, StartRoom, GoalRoom }

    /// <summary>
    /// An encounter fixed to a place on the route in a <see cref="DungeonRecipe"/> (card dungeon-pop-7a), e.g.
    /// "a key lock on the last step" or "the platform in the goal room". Slots are planned before the pool.
    /// </summary>
    [GlobalClass, Tool]
    public partial class EncounterSlot : Resource
    {
        [Export] public DungeonEncounter Encounter { get; set; }

        [Export] public SlotPlace Place { get; set; } = SlotPlace.Anywhere;

        /// <summary>For <see cref="SlotPlace.PathStep"/>: counted from the start (0 = the first corridor), or from the goal when negative (-1 = the last corridor). Objects use the path room with that index.</summary>
        [Export] public int Step { get; set; }

        /// <summary>The target on this route; null when the place doesn't exist on it (a step past the end).</summary>
        public EncounterTarget? TargetOn(DungeonRoute route)
        {
            switch (Place)
            {
                case SlotPlace.StartRoom:
                    return new EncounterTarget(Room: route.Start);
                case SlotPlace.GoalRoom:
                    return new EncounterTarget(Room: route.Goal);
                case SlotPlace.PathStep:
                    // locks count corridors, objects path rooms; a negative step counts back from the end of the corridors
                    int step = Step >= 0 ? Step : route.Steps + Step;
                    return step >= 0 && step <= route.Steps ? new EncounterTarget(Step: step) : null;
                default:
                    return EncounterTarget.Any;
            }
        }
    }
}
