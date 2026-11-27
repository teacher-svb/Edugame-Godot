using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// Brings a held character to a spot and turns it (a plaything's enter spot): its navigation agent first, then
    /// straight for the last metre, placed exactly once within a few centimetres, then a turn to the given heading.
    /// Plain C#: the owner calls <see cref="Step"/> every physics frame until it returns true. Used by
    /// <see cref="PlaythingSeat"/> and <see cref="AvocadoSpot"/>; the owner holds the character
    /// (<c>CharacterStateManager.Hold</c>) meanwhile.
    /// </summary>
    public sealed class PlayerApproach
    {
        const float NearEnough = 0.06f;      // m from the spot: stop walking, place the body exactly
        const float StraightFrom = 1f;       // m from the spot: leave the navigation path, go straight
        const float WalkTimeout = 12f;       // s on the navigation path before going straight
        const float StraightTimeout = 3f;    // s going straight before placing the body anyway
        public const float TurnSeconds = 0.3f;

        enum Stage { Walking, Straight, Turning, Done }

        readonly Character3D _body;
        readonly CharacterController3D _cc;
        readonly NavigationAgent3D _agent;
        Vector3 _target;
        Vector3 _heading;
        Stage _stage;
        double _timer;

        public PlayerApproach(Character3D body, CharacterController3D cc, NavigationAgent3D agent)
        {
            _body = body;
            _cc = cc;
            _agent = agent;
        }

        /// <summary>Starts walking to <paramref name="target"/>, to face <paramref name="heading"/> there (world, Y ignored).</summary>
        public void Start(Vector3 target, Vector3 heading)
        {
            _target = target;
            _heading = new Vector3(heading.X, 0, heading.Z);
            _timer = 0;
            if (_agent != null)
            {
                _agent.TargetPosition = target;
                _stage = Stage.Walking;
            }
            else
                _stage = Stage.Straight;
        }

        /// <summary>One physics frame; true once the character stands on the spot, turned.</summary>
        public bool Step(double delta)
        {
            _timer += delta;
            switch (_stage)
            {
                case Stage.Walking:
                    var next = _agent.GetNextPathPosition();
                    if (Flat(_target - _body.GlobalPosition).Length() < StraightFrom || _agent.IsNavigationFinished() || _timer > WalkTimeout)
                        Switch(Stage.Straight);
                    else
                        _cc.MoveInWorld(next - _body.GlobalPosition);
                    return false;

                case Stage.Straight:
                    var toTarget = Flat(_target - _body.GlobalPosition);
                    if (toTarget.Length() < NearEnough || _timer > StraightTimeout)
                    {
                        _body.GlobalPosition = new Vector3(_target.X, _body.GlobalPosition.Y, _target.Z);
                        if (_heading.LengthSquared() > 0.0001f)
                            _cc.FaceTowards(_target + _heading, TurnSeconds);
                        Switch(Stage.Turning);
                    }
                    else
                        _cc.MoveInWorld(toTarget);
                    return false;

                case Stage.Turning:
                    if (_timer >= TurnSeconds)
                        Switch(Stage.Done);
                    return _stage == Stage.Done;

                default:
                    return true;
            }
        }

        void Switch(Stage stage)
        {
            _stage = stage;
            _timer = 0;
        }

        static Vector3 Flat(Vector3 v) => new(v.X, 0, v.Z);
    }
}
