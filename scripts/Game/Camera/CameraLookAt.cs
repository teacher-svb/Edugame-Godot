
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.Camera
{
    public partial class CameraLookAt : Node3D
    {
        [Export] public Node3D Target { get; set; }
        [Export] public float LerpSpeed { get; set; } = 5f;
        Camera3D _camera;

        public override void _Ready()
        {
            _camera = this.FindAnyObjectByType<Camera3D>(includeSelf: true);
        }

        public override void _Process(double delta)
        {
            if (Target == null) return;
            // the target's origin, not shifted by its scale: the player's VisualRoot squashes to 0.5 on landing, which
            // dipped the aim by half a metre (at rest scale 1 the shift was 0, so the aim is unchanged)
            var pos = Target.GlobalPosition;

            var targetBasis = _camera.GlobalTransform.LookingAt(pos).Basis;
            var currentQuat = new Quaternion(_camera.GlobalBasis.Orthonormalized());
            var targetQuat = new Quaternion(targetBasis.Orthonormalized());
            // clamped: a long frame (a load, a hitch) must not overshoot the target
            float weight = Mathf.Min((float)(LerpSpeed * delta), 1f);
            _camera.GlobalBasis = new Basis(currentQuat.Slerp(targetQuat, weight));
        }
    }
}
