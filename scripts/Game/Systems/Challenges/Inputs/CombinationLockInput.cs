using Godot;

namespace TnT.EduGame.Question
{
    // One spinnable wheel per unknown, cycling through the round's options for it.
    [GlobalClass, Tool]
    public partial class CombinationLockInput : ChallengeInput
    {
        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeInputs/CombinationLock.tscn";
    }
}
