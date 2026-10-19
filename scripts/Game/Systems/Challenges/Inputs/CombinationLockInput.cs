using Godot;

namespace TnT.EduGame.Question
{
    // One spinnable wheel per unknown, cycling through the round's options for it.
    [GlobalClass, Tool]
    public partial class CombinationLockInput : ChallengeInput
    {
        // Carrousel wheel instanced per unknown; its CarrouselValue children are
        // placeholders, the first one is used as template for the round's options.
        // Null means the default carrousel scene.
        [Export] public PackedScene WheelScene { get; set; }

        const string DefaultWheelScenePath = "res://Scenes/UI/carrousel.tscn";

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeInputs/CombinationLock.tscn";

        public PackedScene ResolveWheelScene() => WheelScene ?? GD.Load<PackedScene>(DefaultWheelScenePath);
    }
}
