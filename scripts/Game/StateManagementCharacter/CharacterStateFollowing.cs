using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateFollowing : BaseCharacterState, IStateObject<CharacterStateFollowing.FollowOptions>
    {
        public struct FollowOptions
        {
            public Node3D Target;
        }

        CharacterStateManager _manager;

        public override void _Ready()
        {
            base._Ready();
            _manager = this.FindAncestorOfType<CharacterStateManager>();
        }

        public BaseState GetState(FollowOptions options = default)
        {
            _manager.FollowAgent.Target = options.Target;
            return new BaseState(new() { OnEnter = OnEnter, OnExit = OnExit });
        }

        Task OnEnter()
        {
            _manager.FollowAgent.SetProcess(true);
            return Task.CompletedTask;
        }

        Task OnExit()
        {
            _manager.FollowAgent.Target = null;
            _manager.FollowAgent.SetProcess(false);
            return Task.CompletedTask;
        }
    }
}
