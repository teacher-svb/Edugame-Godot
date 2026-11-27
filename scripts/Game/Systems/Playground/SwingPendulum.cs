using Godot;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// Swings one swing seat as a pendulum computed in code (no physics body): an angle and its speed, gravity and a
    /// little damping every physics frame, applied as a rotation of <see cref="Pivot"/> about its local X axis.
    /// <see cref="Pump"/> (the rider's key) adds speed in the direction it moves, once per half swing, up to
    /// <see cref="MaxAngle"/>; without pumping it dies down by itself, also after the rider jumped off. Pauses with the
    /// game. The swing's own AnimationPlayer clips aren't used for the seats.
    /// </summary>
    [GlobalClass]
    public partial class SwingPendulum : Node
    {
        /// <summary>The seat's pivot (top of the chains); it turns about its local X axis.</summary>
        [Export] public Node3D Pivot { get; set; }
        /// <summary>The seat's marker below the pivot; its distance to the pivot is the pendulum length.</summary>
        [Export] public Node3D Seat { get; set; }
        /// <summary>Highest swing, in degrees either side of hanging straight down.</summary>
        [Export(PropertyHint.Range, "5,60")] public float MaxAngle { get; set; } = 32f;
        /// <summary>Speed one pump adds, in radians per second.</summary>
        [Export] public float PumpSpeed { get; set; } = 0.8f;
        /// <summary>Damping per second; the swing loses about half its height in 1.4 / Damping seconds.</summary>
        [Export] public float Damping { get; set; } = 0.15f;
        /// <summary>Damping while braked (<see cref="Brake"/>: someone caught the seat to get on).</summary>
        [Export] public float BrakeDamping { get; set; } = 3f;

        const float Gravity = 9.8f;
        const float RestSpeed = 0.02f;   // rad/s: below this (near the bottom) it stops

        Basis _restBasis;
        float _length = 1.7f;
        int _lastPumpDirection;          // the direction of the last pump: one pump per half swing

        /// <summary>Angle in radians; positive = forward (the seat's +Z, the way the rider faces).</summary>
        public float Angle { get; private set; }
        /// <summary>Angular speed in radians per second; positive = moving forward.</summary>
        public float AngularVelocity { get; private set; }

        /// <summary>The highest angle (radians) the swing reaches with its current speed.</summary>
        public float Amplitude
        {
            get
            {
                float c = Mathf.Cos(Angle) - AngularVelocity * AngularVelocity * _length / (2f * Gravity);
                return Mathf.Acos(Mathf.Clamp(c, -1f, 1f));
            }
        }

        public override void _Ready()
        {
            if (Pivot == null)
            {
                GD.PrintErr($"SwingPendulum '{Name}': Pivot not set.");
                return;
            }
            _restBasis = Pivot.Basis;
            if (Seat != null)
                _length = Mathf.Max(0.3f, Pivot.GlobalPosition.DistanceTo(Seat.GlobalPosition));
        }

        /// <summary>
        /// Adds <see cref="PumpSpeed"/> in the direction the seat moves (forward when it hangs still). Ignored when the
        /// last pump was in the same direction: one pump per half swing, so pressing in rhythm swings higher, mashing
        /// doesn't. The swing never goes higher than <see cref="MaxAngle"/>.
        /// </summary>
        public bool Pump()
        {
            _braking = false;
            int direction = AngularVelocity > RestSpeed ? 1 : AngularVelocity < -RestSpeed ? -1 : 1;
            if (direction == _lastPumpDirection)
                return false;
            _lastPumpDirection = direction;
            AngularVelocity += direction * PumpSpeed;

            // cap: the speed at this angle that reaches MaxAngle and no higher
            float max = Mathf.DegToRad(MaxAngle);
            float room = 2f * Gravity / _length * (Mathf.Cos(Angle) - Mathf.Cos(max));
            float cap = room > 0 ? Mathf.Sqrt(room) : 0f;
            AngularVelocity = Mathf.Clamp(AngularVelocity, -cap, cap);
            return true;
        }

        /// <summary>Stops the swing hanging straight down (no swing back).</summary>
        /// <summary>Slows the swing down quickly (<see cref="BrakeDamping"/>) until it hangs still or is pumped: the
        /// player catching a still-moving seat to get on.</summary>
        public void Brake() => _braking = true;

        bool _braking;

        public void Stop()
        {
            _braking = false;
            Angle = 0;
            AngularVelocity = 0;
            _lastPumpDirection = 0;
            Apply();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Pivot == null || (Angle == 0 && AngularVelocity == 0))
                return;

            // semi-implicit Euler, a few steps per frame: stable for the pendulum
            const int steps = 4;
            float dt = (float)delta / steps;
            for (int i = 0; i < steps; i++)
            {
                AngularVelocity += (-Gravity / _length * Mathf.Sin(Angle) - (_braking ? Mathf.Max(Damping, BrakeDamping) : Damping) * AngularVelocity) * dt;
                Angle += AngularVelocity * dt;
            }
            if (Mathf.Abs(AngularVelocity) < RestSpeed && Mathf.Abs(Angle) < 0.002f)
            {
                Stop();
                return;
            }
            // a new half swing allows a new pump in that direction
            if (_lastPumpDirection != 0 && Mathf.Sign(AngularVelocity) != _lastPumpDirection)
                _lastPumpDirection = 0;
            Apply();
        }

        // forward (+Z) is a negative turn about X: (0, -L, 0) turned by -θ about X points to +Z
        void Apply() => Pivot.Basis = _restBasis * new Basis(Vector3.Right, -Angle);
    }
}
