
using Godot;
using TnT.Extensions;

public partial class CameraLookat : Node3D
{
    [Export] public Node3D Target { get; set; }
    [Export] public float LerpSpeed { get; set; } = 5f;
    Camera3D _camera;

    public override void _Ready()
    {
        _camera = this.FindAnyObjectByType<Camera3D>();
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;
        var pos = Target.GlobalPosition;

        pos.Y += 1 * Target.Scale.Y;
        pos.Y -= 1;

        var targetBasis = _camera.GlobalTransform.LookingAt(pos).Basis;
        var currentQuat = new Quaternion(_camera.GlobalBasis.Orthonormalized());
        var targetQuat = new Quaternion(targetBasis.Orthonormalized());
        _camera.GlobalBasis = new Basis(currentQuat.Slerp(targetQuat, (float)(LerpSpeed * delta)));
    }
}
