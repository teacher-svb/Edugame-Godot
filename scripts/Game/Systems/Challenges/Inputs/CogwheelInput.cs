using Godot;

namespace TnT.EduGame.Question
{
    // A shared row of numbers (all unknowns' options combined); each pick fills the next
    // empty unknown, picking again takes it back. Pair with a DistinctParams constraint:
    // a value needed twice can't be picked twice.
    [GlobalClass, Tool]
    public partial class CogwheelInput : ChallengeInput
    {
        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeInputs/Cogwheel.tscn";
    }
}
