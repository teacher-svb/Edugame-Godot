using Godot;

namespace TnT.EduGame.Question
{
    // One dropdown per unknown, listing the round's options for it.
    [GlobalClass, Tool]
    public partial class DropdownInput : ChallengeInput
    {
        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeInputs/Dropdown.tscn";
    }
}
