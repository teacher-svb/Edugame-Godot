using Godot;

namespace TnT.EduGame.Platforming
{
    /// <summary>
    /// A dangerous area (fire, a falling rock): while <see cref="Active"/>, a player inside it is hit and respawned by
    /// <see cref="ChamberSafety"/>. Animations switch <see cref="Active"/> on and off; the area keeps monitoring, so a
    /// player already standing inside is hit the moment it turns on.
    /// </summary>
    [GlobalClass]
    public partial class ChallengeHazard : Area3D
    {
        [Export] public bool Active { get; set; } = true;

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
                if (body is Node3D n && ChamberSafety.IsPlayer(n))
                {
                    ChamberSafety.Instance?.Hit();
                    return;
                }
            }
        }
    }
}
