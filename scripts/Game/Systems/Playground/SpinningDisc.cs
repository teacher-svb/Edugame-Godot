using Godot;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// Turns a merry-go-round's disc in code (no physics body): a turning speed that friction slows every physics
    /// frame, applied as a rotation of <see cref="Disc"/> about its local Y axis (positive: counter-clockwise seen
    /// from above). <see cref="Push"/> (the rider's key, pushing off from the deck) adds speed, at most once per
    /// <see cref="PushInterval"/>, up to <see cref="MaxSpeed"/>; without pushing it slows down and stops, also after
    /// the rider jumped off. Pauses with the game. The GLB's <c>mgr_spin</c> clip isn't used.
    /// </summary>
    [GlobalClass]
    public partial class SpinningDisc : Node
    {
        /// <summary>The disc that turns (with the riders' markers under it); it turns about its local Y axis.</summary>
        [Export] public Node3D Disc { get; set; }
        /// <summary>Highest turning speed, radians per second (3 ≈ one turn in 2.1 s).</summary>
        [Export] public float MaxSpeed { get; set; } = 3f;
        /// <summary>Speed one push adds, radians per second.</summary>
        [Export] public float PushSpeed { get; set; } = 0.8f;
        /// <summary>Seconds after a push before the next one counts: pushing in rhythm speeds it up, mashing doesn't.</summary>
        [Export] public float PushInterval { get; set; } = 0.45f;
        /// <summary>Friction per second, relative to the speed (the disc loses about half its speed in 0.7 / Friction s).</summary>
        [Export] public float Friction { get; set; } = 0.12f;
        /// <summary>Constant slowing, radians per second², so it comes to a stop instead of creeping on.</summary>
        [Export] public float StopDrag { get; set; } = 0.05f;

        Basis _restBasis;
        double _sincePush = double.MaxValue;

        /// <summary>The disc's turn from its rest, in radians.</summary>
        public float Angle { get; private set; }
        /// <summary>Turning speed in radians per second (positive: counter-clockwise seen from above).</summary>
        public float AngularVelocity { get; private set; }

        /// <summary>Speed in m/s of a point on the disc at <paramref name="worldPosition"/>.</summary>
        public float SpeedAt(Vector3 worldPosition)
        {
            if (Disc == null)
                return 0f;
            var r = worldPosition - Disc.GlobalPosition;
            r.Y = 0;
            return Mathf.Abs(AngularVelocity) * r.Length();
        }

        public override void _Ready()
        {
            if (Disc == null)
            {
                GD.PrintErr($"SpinningDisc '{Name}': Disc not set.");
                return;
            }
            _restBasis = Disc.Basis;
        }

        /// <summary>
        /// Adds <see cref="PushSpeed"/> (counter-clockwise), up to <see cref="MaxSpeed"/>. False, and nothing added,
        /// within <see cref="PushInterval"/> of the last push that counted.
        /// </summary>
        public bool Push()
        {
            if (_sincePush < PushInterval)
                return false;
            _sincePush = 0;
            AngularVelocity = Mathf.Min(AngularVelocity + PushSpeed, MaxSpeed);
            return true;
        }

        /// <summary>Stops the disc where it is.</summary>
        public void Stop() => AngularVelocity = 0;

        public override void _PhysicsProcess(double delta)
        {
            _sincePush += delta;
            if (Disc == null || AngularVelocity == 0)
                return;
            float dt = (float)delta;
            float slow = (Friction * Mathf.Abs(AngularVelocity) + StopDrag) * dt;
            AngularVelocity = Mathf.Abs(AngularVelocity) <= slow ? 0 : AngularVelocity - Mathf.Sign(AngularVelocity) * slow;
            Angle = Mathf.Wrap(Angle + AngularVelocity * dt, 0, Mathf.Tau);
            Disc.Basis = _restBasis * new Basis(Vector3.Up, Angle);
        }
    }
}
