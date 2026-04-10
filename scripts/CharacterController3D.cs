// CharacterController3D.cs — knows nothing about animations
using Godot;
using TnT.Systems;

public partial class CharacterController3D : CharacterBody3D, ICharacterController
{
    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;
    public const float RotationSpeed = 10.0f;

    [Export] public Node3D VisualRoot { get; set; }

    [Signal] public delegate void MovementStateChangedEventHandler(string state);

    private string _currentState = "";

    Vector2 inputDir;

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
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
        VisualRoot.Basis = VisualRoot.Basis.Slerp(target, RotationSpeed * (float)delta);
    }

    private void EmitMovementState(Vector3 direction)
    {
        string state;

        if (!IsOnFloor())
            state = Velocity.Y > 0 ? "airborne_rising" : "airborne_falling";
        else if (direction != Vector3.Zero)
            state = "moving";
        else
            state = "idle";

        if (state == _currentState) return;
        _currentState = state;
        EmitSignal(SignalName.MovementStateChanged, state);
    }

    public void Move(Vector2 movement)
    {
        inputDir = movement;
    }

    public void MoveTo(Vector3 position)
    {
        this.Position = position;
    }
}