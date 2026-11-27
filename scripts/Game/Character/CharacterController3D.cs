using Godot;
using TnT.Extensions;

namespace TnT.EduGame.Characters
{
    /// <summary>
    /// 3D character controller that handles physics-based movement, jumping, and rotation
    /// for a <see cref="CharacterBody3D"/>. Implements <see cref="ICharacterController"/>
    /// so it can be driven by the game's character and input systems.
    /// </summary>
    [GlobalClass]
    public partial class CharacterController3D : Node, ICharacterController
    {
        [Export] private float _speed = 5.0f;
        [Export] private float _jumpVelocity = 4.5f;
        [Export] private float _rotationSpeed = 10.0f;
        /// <summary>Seconds a jump pressed in the air is kept, so it still fires when the character lands right after.</summary>
        [Export] private float _jumpBufferSeconds = 0.15f;
        Character3D _character;
        public Vector3 GlobalPosition => _character.GlobalPosition;


        /// <summary>
        /// The camera that follows or is attached to this character.
        /// </summary>
        [Export] public Camera3D Camera { get; private set; }

        [Export] public Node3D VisualRoot { get; private set; }
        public float Speed { get => _speed; set => _speed = value; }

        /// <summary>
        /// Emitted whenever the character's movement state changes.
        /// Possible state values are <c>"idle"</c>, <c>"run"</c>, <c>"jump"</c>,
        /// <c>"fall"</c> and <c>"land"</c>. Repeats are suppressed, so <c>"jump"</c>
        /// fires once per jump and <c>"land"</c> once per touchdown.
        /// </summary>
        [Signal] public delegate void MovementStateChangedEventHandler(string state);

        private string _currentState = "";
        private bool _wasOnFloor = true;
        private bool _justJumped = false;

        Vector2 inputDir;
        // a world-space walk direction (NPCs on a navigation path); wins over inputDir, cleared every frame
        Vector3 _worldDir;

        // time left on a requested jump: it fires on the floor within that time, or is dropped (finding 4.12: a
        // jump pressed in the air used to fire on the next landing, however late, also after a teleport)
        double _jumpBufferLeft = 0;

        Vector3 _push;

        public override void _Ready()
        {
            _character = this.FindAncestorOfType<Character3D>();
        }

        /// <summary>
        /// False: the body isn't moved at all (no gravity, input, walk or push) and no movement state is emitted, for a
        /// scripted sequence that places the body itself (sitting on a plaything). Turning it off drops the velocity
        /// and this frame's queued input; true again: physics as usual, from where the body was put.
        /// </summary>
        public bool MovementEnabled
        {
            get => IsPhysicsProcessing();
            set
            {
                if (!value && _character != null)
                {
                    _character.Velocity = Vector3.Zero;
                    _jumpBufferLeft = 0;
                    inputDir = Vector2.Zero;
                    _worldDir = Vector3.Zero;
                    _push = Vector3.Zero;
                }
                SetPhysicsProcess(value);
            }
        }

        /// <summary>
        /// Processes physics each frame: applies gravity, handles jumping,
        /// moves the character, rotates the visual root toward the direction of travel,
        /// and emits movement state changes.
        /// </summary>
        public override void _PhysicsProcess(double delta)
        {
            Vector3 velocity = _character.Velocity;

            if (_character.IsOnFloor() == false)
                velocity += _character.GetGravity() * (float)delta;

            if (_jumpBufferLeft > 0)
            {
                if (_character.IsOnFloor())
                {
                    velocity.Y = _jumpVelocity;
                    _jumpBufferLeft = 0;
                    _justJumped = true;
                }
                else
                    _jumpBufferLeft -= delta;
            }

            // Move: in the body's space (the player's input); MoveInWorld: as given, whatever the body's rotation.
            // The body's global rotation, not its local Transform.Basis: under a rotated parent the input turned by the
            // local basis only, so it no longer matched the camera's directions (scale left out)
            Vector3 direction = _worldDir != Vector3.Zero
                ? new Vector3(_worldDir.X, 0, _worldDir.Z).Normalized()
                : (_character.GlobalBasis.Orthonormalized() * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * _speed;
                velocity.Z = direction.Z * _speed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(_character.Velocity.X, 0, _speed);
                velocity.Z = Mathf.MoveToward(_character.Velocity.Z, 0, _speed);
            }

            // external push (e.g. a waterfall), added on top of the player's own movement; sources add it every frame
            velocity.X += _push.X;
            velocity.Z += _push.Z;
            _push = Vector3.Zero;

            _character.Velocity = velocity;
            _character.MoveAndSlide();

            if (direction != Vector3.Zero)
                OrientToDirection(direction, delta);

            EmitMovementState(direction);
            inputDir = Vector2.Zero;
            _worldDir = Vector3.Zero;
        }

        // a paused tree (message, menu, challenge) stops this node in its last movement state, while VisualRoot (its
        // AnimationTree) has process mode Always: a character that was running kept its run animation in place. On
        // the floor it goes to idle; after the pause the next physics frame emits its real state again
        public override void _Notification(int what)
        {
            if (what == NotificationPaused && _character != null && _character.IsOnFloor() && _currentState != "idle")
            {
                _currentState = "idle";
                EmitSignal(SignalName.MovementStateChanged, _currentState);
            }
        }

        /// <summary>
        /// Turns <see cref="VisualRoot"/> to face <paramref name="worldPosition"/> (horizontally) over
        /// <paramref name="duration"/> seconds, also while the tree is paused (e.g. a character turning to the player
        /// when it starts to speak). Walking turns it again toward the walk direction.
        /// </summary>
        public void FaceTowards(Vector3 worldPosition, float duration = 0.25f)
        {
            if (VisualRoot == null || !VisualRoot.IsInsideTree())
                return;
            var direction = worldPosition - VisualRoot.GlobalPosition;
            direction.Y = 0;
            if (direction.LengthSquared() < 0.0001f)
                return;
            var target = FacingQuaternion(direction.Normalized());
            _faceTween?.Kill();
            if (duration <= 0)
            {
                VisualRoot.Quaternion = target;
                return;
            }
            VisualRoot.Quaternion = VisualRoot.Quaternion.Normalized();   // the tween slerps: needs a unit start
            _faceTween = VisualRoot.CreateTween();
            _faceTween.SetPauseMode(Tween.TweenPauseMode.Process);
            _faceTween.TweenProperty(VisualRoot, "quaternion", target, duration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }

        Tween _faceTween;

        // world-space facing direction → VisualRoot's quaternion, relative to its parent
        Quaternion FacingQuaternion(Vector3 direction)
        {
            var target = new Quaternion(Basis.LookingAt(-direction, Vector3.Up));
            if (VisualRoot.GetParentOrNull<Node3D>() is Node3D parent)
                target = (parent.GlobalBasis.GetRotationQuaternion().Inverse() * target).Normalized();
            return target;
        }

        private void OrientToDirection(Vector3 direction, double delta)
        {
            if (VisualRoot == null) return;
            // through Quaternion, which keeps the node's scale: setting an orthonormalized Basis reset it to 1
            // (finding 4.8: the Earth wizard's Mage2 at 0.525 grew to full size once he walked)
            // direction is world space, the quaternion is relative to VisualRoot's parent: turn the world facing
            // into the parent's space, so a character whose body is rotated in the level faces the right way
            var target = FacingQuaternion(direction);
            // walking takes over from a FaceTowards turn
            if (_faceTween != null && _faceTween.IsValid())
            {
                _faceTween.Kill();
                _faceTween = null;
            }
            float weight = Mathf.Min(_rotationSpeed * (float)delta, 1f);
            VisualRoot.Quaternion = VisualRoot.Quaternion.Normalized().Slerp(target, weight);
        }

        private void EmitMovementState(Vector3 direction)
        {
            string state;

            if (_justJumped)
            {
                state = "jump";
                _justJumped = false;
            }
            else if (!_wasOnFloor && _character.IsOnFloor())
                state = "land";
            else if (_character.IsOnFloor() == false)
                state = _character.Velocity.Y > 0 ? "jump" : "fall";
            else if (direction != Vector3.Zero)
                state = "run";
            else
                state = "idle";

            _wasOnFloor = _character.IsOnFloor();

            if (state == _currentState) return;
            _currentState = state;
            EmitSignal(SignalName.MovementStateChanged, state);
        }

        /// <summary>
        /// Pushes the character horizontally during the next physics frame, on top of its own movement.
        /// Call it every physics frame while the push lasts (pushes from several sources add up).
        /// </summary>
        /// <param name="velocity">Push velocity in world space (m/s); the Y component is ignored.</param>
        public void AddPush(Vector3 velocity)
        {
            _push += velocity;
        }

        /// <summary>Jumps now when on the floor, or on landing within the jump buffer time.</summary>
        public void Jump()
        {
            _jumpBufferLeft = _jumpBufferSeconds;
        }

        /// <summary>
        /// Queues a movement input to be applied during the next physics frame.
        /// </summary>
        /// <param name="movement">
        /// A 2D direction vector where X maps to the body's right/left axis
        /// and Y maps to its forward/back axis (the body's global rotation, its parents' included).
        /// </param>
        public void Move(Vector2 movement)
        {
            inputDir = movement;
            _worldDir = Vector3.Zero;
        }

        /// <summary>
        /// Queues a walk in a world-space direction for the next physics frame (Y ignored, any length: the character
        /// walks at its speed), not turned by the body's rotation as <see cref="Move"/> is. For NPCs following a
        /// navigation path, whose next path point is a world position: with <see cref="Move"/> a character whose
        /// body is rotated in the level walked in a rotated direction.
        /// </summary>
        public void MoveInWorld(Vector3 direction)
        {
            _worldDir = direction;
            inputDir = Vector2.Zero;
        }

        /// <summary>
        /// Teleports the character instantly to the specified world-space position, standing still: its velocity,
        /// a buffered jump and this frame's queued input and push are dropped.
        /// </summary>
        /// <param name="position">The target position in world space.</param>
        public void MoveTo(Vector3 position)
        {
            _character.GlobalPosition = position;
            _character.Velocity = Vector3.Zero;
            _jumpBufferLeft = 0;
            inputDir = Vector2.Zero;
            _worldDir = Vector3.Zero;
            _push = Vector3.Zero;
        }
    }
}
