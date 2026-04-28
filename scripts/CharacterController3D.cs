using Godot;
using TnT.Systems;

/// <summary>
/// 3D character controller that handles physics-based movement, jumping, and rotation
/// for a <see cref="CharacterBody3D"/>. Implements <see cref="ICharacterController"/>
/// so it can be driven by the game's character and input systems.
/// </summary>
public partial class CharacterController3D : CharacterBody3D, ICharacterController
{
    [Export] private float _speed = 5.0f;
    [Export] private float _jumpVelocity = 4.5f;
    [Export] private float _rotationSpeed = 10.0f;


    /// <summary>
    /// Raycast used to detect occlusion between the camera and the character.
    /// </summary>
    [Export] public RayCast3D OcclusionRaycast { get; private set; }

    /// <summary>
    /// The camera that follows or is attached to this character.
    /// </summary>
    [Export] public Camera3D Camera { get; private set; }

    [Export] private Node3D VisualRoot { get; set; }
    public float Speed { get => _speed; set => _speed = value; }

    /// <summary>
    /// Emitted whenever the character's movement state changes.
    /// Possible state values are <c>"idle"</c>, <c>"moving"</c>,
    /// <c>"jumped"</c>, <c>"landed"</c>, <c>"airborne_rising"</c>, and <c>"airborne_falling"</c>.
    /// <c>"jumped"</c> and <c>"landed"</c> fire for exactly one physics frame.
    /// </summary>
    [Signal] public delegate void MovementStateChangedEventHandler(string state);

    private string _currentState = "";
    private bool _wasOnFloor = true;
    private bool _justJumped = false;

    Vector2 inputDir;

    bool _jump = false;

    public override void _Ready()
    {

        if (OcclusionRaycast != null)
            OcclusionRaycast.AddException(this);
    }

    /// <summary>
    /// Processes physics each frame: applies gravity, handles jumping,
    /// moves the character, rotates the visual root toward the direction of travel,
    /// and emits movement state changes.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (IsOnFloor() && _jump)
        {
            velocity.Y = _jumpVelocity;
            _jump = false;
            _justJumped = true;
        }

        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _speed;
            velocity.Z = direction.Z * _speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _speed);
        }

        Velocity = velocity;
        MoveAndSlide();

        if (direction != Vector3.Zero)
            OrientToDirection(direction, delta);

        EmitMovementState(direction);
        inputDir = Vector2.Zero;
    }

    private void OrientToDirection(Vector3 direction, double delta)
    {
        if (VisualRoot == null) return;
        var target = Basis.LookingAt(-direction, Vector3.Up);
        VisualRoot.Basis = VisualRoot.Basis.Orthonormalized().Slerp(target, _rotationSpeed * (float)delta);
    }

    private void EmitMovementState(Vector3 direction)
    {
        string state;

        if (_justJumped)
        {
            state = "jumped";
            _justJumped = false;
        }
        else if (!_wasOnFloor && IsOnFloor())
            state = "landed";
        else if (!IsOnFloor())
            state = Velocity.Y > 0 ? "jump" : "fall";
        else if (direction != Vector3.Zero)
            state = "walk";
        else
            state = "idle";

        _wasOnFloor = IsOnFloor();

        if (state == _currentState) return;
        _currentState = state;
        EmitSignal(SignalName.MovementStateChanged, state);
    }

    public void Jump()
    {
        _jump = true;
    }

    /// <summary>
    /// Queues a movement input to be applied during the next physics frame.
    /// </summary>
    /// <param name="movement">
    /// A 2D direction vector where X maps to the local right/left axis
    /// and Y maps to the local forward/back axis.
    /// </param>
    public void Move(Vector2 movement)
    {
        inputDir = movement;
    }

    /// <summary>
    /// Teleports the character instantly to the specified world-space position.
    /// </summary>
    /// <param name="position">The target position in world space.</param>
    public void MoveTo(Vector3 position)
    {
        this.Position = position;
    }
}