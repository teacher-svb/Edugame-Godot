using System.Linq;
using Godot;
using TnT.EduGame;
using TnT.Input;
using TnT.Systems.Persistence;

public partial class PlayerTutorial : Node3D, IBind<TutorialSaveData>
{
    [Export] Node3D _arrow;
    [Export] Label3D _label;
    public InputAction2D MoveAction { get; set; }

    TutorialSaveData _saveData;
    public string PersistentId { get; private set; }

    public override void _EnterTree()
    {
        PersistentId = GetPath().ToString();
    }

    public void Bind(TutorialSaveData data)
    {
        _saveData = data;
        Visible = !data.IsCompleted;
    }

    public void MarkCompleted()
    {
        if (_saveData != null)
            _saveData.IsCompleted = true;
        Hide();
    }

    string GetKeyText(InputActionBase action)
    {
        var @event = InputMap.ActionGetEvents(action.ActionName).FirstOrDefault(e => e is InputEventKey) as InputEventKey;
        var key = DisplayServer.KeyboardGetKeycodeFromPhysical(@event.PhysicalKeycode);
        return OS.GetKeycodeString(key);
    }

    public void ShowDirection(Vector2I direction, InputActionBase action)
    {
        Visible = true;
        _label.Text = GetKeyText(action);
        // Atan2(X, -Y) maps Vector2I directions to Y-axis rotation.
        // Adjust if your arrow's default forward differs from -Z.
        float angle = Mathf.Atan2(direction.X, -direction.Y);
        _arrow.Rotation = _arrow.Rotation with { Y = angle };
    }

    public void ShowDirectionDown()  => ShowDirection(Vector2I.Down,  MoveAction.PositiveY);
    public void ShowDirectionUp()    => ShowDirection(Vector2I.Up,    MoveAction.NegativeY);
    public void ShowDirectionRight() => ShowDirection(Vector2I.Right, MoveAction.PositiveX);
    public void ShowDirectionLeft()  => ShowDirection(Vector2I.Left,  MoveAction.NegativeX);
}
