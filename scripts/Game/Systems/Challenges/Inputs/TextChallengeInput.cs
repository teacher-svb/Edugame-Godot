using Godot;

namespace TnT.EduGame.Question
{
    // One numeric field per unknown.
    [GlobalClass, Tool]
    public partial class TextChallengeInput : ChallengeInput
    {
        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeInputs/TextInput.tscn";
    }
}
