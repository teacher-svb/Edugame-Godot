using Godot;

namespace TnT.EduGame.Question
{
    // Shared base of the pluggable parts of a challenge (visualizations, inputs):
    // a resource with its own settings that knows which widget scene renders it.
    [GlobalClass, Tool]
    public abstract partial class ChallengeElement : ParamAwareResource
    {
        // optional override of the subclass' default scene
        [Export] public PackedScene Scene { get; set; }

        protected abstract string DefaultScenePath { get; }

        public PackedScene ResolveScene() => Scene ?? GD.Load<PackedScene>(DefaultScenePath);
    }
}
