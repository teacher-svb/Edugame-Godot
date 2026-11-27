using Godot;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// Rocks a seesaw beam in code: at rest the player's end is down (the GLB's rest pose). <see cref="Push"/> (the
    /// rider's key) starts one bounce from rest: the beam tilts <see cref="TiltDegrees"/> up to the other end (that end
    /// on the ground), stays there a moment and comes back down. One bounce per press; a press during a bounce does
    /// nothing. Pauses with the game. The GLB's <c>seesaw</c> clip isn't used.
    /// </summary>
    [GlobalClass]
    public partial class SeesawBeam : Node
    {
        /// <summary>The beam; it turns about its local Z axis (positive raises its +X end).</summary>
        [Export] public Node3D Beam { get; set; }
        /// <summary>From rest to the other end on the ground, degrees (the GLB: 2 × 11.5).</summary>
        [Export] public float TiltDegrees { get; set; } = 23f;
        /// <summary>One bounce: up, the other end on the ground, down again.</summary>
        [Export] public float BounceSeconds { get; set; } = 1.2f;

        const float Up = 0.42f, Top = 0.58f;   // the bounce's thirds: going up, at the other end, coming down

        Basis _restBasis;

        /// <summary>0 at rest, then 0 → 1 through a bounce.</summary>
        public float Progress { get; private set; }
        /// <summary>Not bouncing: the player's end is down.</summary>
        public bool AtRest => Progress == 0;
        /// <summary>How far the beam is tilted from rest, 0 (rest) to 1 (the other end down).</summary>
        public float Tilt { get; private set; }

        public override void _Ready()
        {
            if (Beam == null)
            {
                GD.PrintErr($"SeesawBeam '{Name}': Beam not set.");
                return;
            }
            _restBasis = Beam.Basis;
        }

        /// <summary>Starts a bounce; false (and nothing happens) while one runs.</summary>
        public bool Push()
        {
            if (Beam == null || !AtRest)
                return false;
            Progress = 0.0001f;
            return true;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Beam == null || AtRest)
                return;
            Progress += (float)delta / Mathf.Max(0.1f, BounceSeconds);
            if (Progress >= 1f)
            {
                Progress = 0;
                Tilt = 0;
            }
            else if (Progress < Up)
            {
                float u = Progress / Up;
                Tilt = 1f - (1f - u) * (1f - u);   // pushed off: fast, slowing near the top
            }
            else if (Progress < Top)
                Tilt = 1f;
            else
            {
                float u = (Progress - Top) / (1f - Top);
                Tilt = 1f - u * u;                 // falling back: slow, then faster
            }
            // the player's end (−X) goes up: a negative turn about Z from the rest tilt
            Beam.Basis = _restBasis * new Basis(Vector3.Back, -Mathf.DegToRad(TiltDegrees) * Tilt);
        }
    }
}
