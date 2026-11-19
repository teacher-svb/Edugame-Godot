using Godot;

namespace TnT.EduGame.Platforming
{
    /// <summary>
    /// While <see cref="Active"/>, pushes a player inside the area back along the zone's local Z axis, away from its
    /// centre: whichever end the player came from, they are pushed back towards it (a waterfall across a bridge).
    /// With <see cref="Strength"/> above the player's speed the player cannot pass while it runs.
    /// </summary>
    [GlobalClass]
    public partial class PushZone : Area3D
    {
        [Export] public bool Active { get; set; } = true;

        /// <summary>Push speed in m/s (the player walks at 3 m/s).</summary>
        [Export] public float Strength { get; set; } = 4.5f;

        public override void _Ready()
        {
            Monitoring = true;
            Monitorable = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Active) return;
            foreach (var body in GetOverlappingBodies())
            {
                if (body is not Node3D n || !ChamberSafety.IsPlayer(n)) continue;
                var controller = Player.Instance?.GetParent()?.GetNodeOrNull<CharacterController3D>("CharacterController3D");
                if (controller == null) return;
                var axis = GlobalBasis.Z.Normalized();
                float side = axis.Dot(n.GlobalPosition - GlobalPosition);
                controller.AddPush(axis * (side >= 0 ? Strength : -Strength));
                return;
            }
        }
    }
}
