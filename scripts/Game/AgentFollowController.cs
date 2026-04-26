using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

[GlobalClass]
public partial class AgentFollowController : Node
{
    public Node3D Target { get; set; }
	[Export] NavigationAgent3D _agent;

    Vector3 _direction;
    CharacterController3D _cc;

    public override void _Ready()
    {
        _cc = this.FindAncestorOfType<CharacterController3D>();
        SetProcess(false);
    }

    public async override void _Process(double delta)
    {
        if (Target == null)
            return;

        _direction = _agent.GetNextPathPosition() - _cc.GlobalPosition;
        _cc.Move(_direction.ToVector2XZ());

        await FindPath();
    }
	async Task FindPath()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        _agent.TargetPosition = Target.GlobalPosition;
	}
}
