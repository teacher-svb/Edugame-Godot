using Godot;
using TnT.EduGame.GameState;
using TnT.Input;

public partial class PlayerTutorialLabel : Node
{
    InputAction2D _movement = StateManagerGame.Instance.MovePlayerAction;
    InputAction _jump = StateManagerGame.Instance.JumpPlayerAction;
}