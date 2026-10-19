using Godot;

namespace TnT.EduGame.Question
{
    // How a challenge is shown. Concrete subclasses carry their own settings
    // (grid size, image, ring count, ...) and point at the scene that renders
    // them. A visualization also decides the round's values: the default rolls
    // every param from its ParamSpec, subclasses override Generate to add their
    // own constraints (e.g. SearchGrid clamping to the grid bounds).
    [GlobalClass, Tool]
    public abstract partial class ChallengeVisualization : Resource
    {
        // optional override of the subclass' default scene
        [Export] public PackedScene Scene { get; set; }

        protected abstract string DefaultScenePath { get; }

        public PackedScene ResolveScene() => Scene ?? GD.Load<PackedScene>(DefaultScenePath);

        public virtual ChallengeRound Generate(MathChallenge challenge, int seed) =>
            ChallengeRound.Roll(challenge.Formula, challenge.Params, challenge.Unknowns, seed, challenge.AllowNegativeAnswer, challenge.DistinctValues);
    }
}
