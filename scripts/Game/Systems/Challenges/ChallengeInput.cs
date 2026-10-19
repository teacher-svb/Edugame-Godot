using Godot;

namespace TnT.EduGame.Question
{
    // How the player supplies values for a round's unknowns. Concrete subclasses
    // carry their own settings and point at the scene that renders them.
    [GlobalClass, Tool]
    public abstract partial class ChallengeInput : Resource
    {
        // optional override of the subclass' default scene
        [Export] public PackedScene Scene { get; set; }

        protected abstract string DefaultScenePath { get; }

        public PackedScene ResolveScene() => Scene ?? GD.Load<PackedScene>(DefaultScenePath);
    }
}
