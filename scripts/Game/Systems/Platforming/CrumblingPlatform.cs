using Godot;

namespace TnT.EduGame.Platforming
{
    /// <summary>
    /// A small platform that shakes once the player stands on it, then falls away, and comes back after a while.
    /// Children: a <see cref="Visual"/> node to shake and a <see cref="Sensor"/> area on top that detects the player.
    /// </summary>
    [GlobalClass]
    public partial class CrumblingPlatform : AnimatableBody3D
    {
        [Export] public Node3D Visual { get; set; }
        [Export] public Area3D Sensor { get; set; }

        /// <summary>Seconds of shaking after the player lands, before it falls.</summary>
        [Export] public float Delay { get; set; } = 0.6f;

        /// <summary>Seconds after falling before it is back in place.</summary>
        [Export] public float ResetTime { get; set; } = 3.0f;

        enum State { Idle, Shaking, Falling, Gone }

        State _state = State.Idle;
        double _t;
        float _fallSpeed;
        Vector3 _home;
        Vector3 _visualHome;
        readonly RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _home = Position;
            if (Visual != null) _visualHome = Visual.Position;
            SyncToPhysics = true;
        }

        public override void _PhysicsProcess(double delta)
        {
            _t += delta;
            switch (_state)
            {
                case State.Idle:
                    if (PlayerOnTop()) Go(State.Shaking);
                    break;
                case State.Shaking:
                    if (Visual != null)
                        Visual.Position = _visualHome + new Vector3(_rng.RandfRange(-0.03f, 0.03f), 0, _rng.RandfRange(-0.03f, 0.03f));
                    if (_t >= Delay)
                    {
                        if (Visual != null) Visual.Position = _visualHome;
                        _fallSpeed = 0;
                        Go(State.Falling);
                    }
                    break;
                case State.Falling:
                    _fallSpeed += 9.8f * (float)delta;
                    Position += Vector3.Down * _fallSpeed * (float)delta;
                    if (_t > 0.9)
                    {
                        Visible = false;
                        SetCollision(false);
                        Go(State.Gone);
                    }
                    break;
                case State.Gone:
                    if (_t >= ResetTime && !PlayerNearHome())
                    {
                        Position = _home;
                        Visible = true;
                        SetCollision(true);
                        Go(State.Idle);
                    }
                    break;
            }
        }

        void Go(State s)
        {
            _state = s;
            _t = 0;
        }

        bool PlayerOnTop()
        {
            if (Sensor == null) return false;
            foreach (var body in Sensor.GetOverlappingBodies())
                if (body is Node3D n && ChamberSafety.IsPlayer(n)) return true;
            return false;
        }

        bool PlayerNearHome()
        {
            var body = ChamberSafety.PlayerBody;
            if (body == null) return false;
            var home = GetParent<Node3D>().ToGlobal(_home);
            return body.GlobalPosition.DistanceTo(home) < 0.6f;
        }

        void SetCollision(bool on)
        {
            foreach (var child in GetChildren())
                if (child is CollisionShape3D cs) cs.SetDeferred(CollisionShape3D.PropertyName.Disabled, !on);
        }
    }
}
