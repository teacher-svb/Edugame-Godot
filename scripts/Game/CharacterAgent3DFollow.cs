using Godot;
using TnT.Extensions;

[GlobalClass]
public partial class CharacterAgent3DFollow : NavigationAgent3D
{
    public Node3D Target { get; set; }

    Vector3 _direction;
    CharacterController3D _cc;

    public override void _Ready()
    {
        _cc = this.FindAncestorOfType<CharacterController3D>();
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (Target == null)
            return;

        TargetPosition = Target.GlobalPosition;
        _direction = GetNextPathPosition() - _cc.GlobalPosition;
        _cc.Move(_direction.ToVector2XZ());
    }
}
