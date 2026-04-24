using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStatePatrolling : BaseCharacterState, IStateObject<CharacterStatePatrolling.PatrolOptions>
    {
        public struct PatrolOptions { }

        CharacterStateManager _manager;

        public override void _Ready()
        {
            base._Ready();
            _manager = this.FindAncestorOfType<CharacterStateManager>();
        }

        public BaseState GetState(PatrolOptions options = default)
        {
            return new BaseState(new() { OnEnter = OnEnter, OnExit = OnExit });
        }

        Task OnEnter()
        {
            _manager.PatrolAgent.SetProcess(true);
            return Task.CompletedTask;
        }

        Task OnExit()
        {
            _manager.PatrolAgent.SetProcess(false);
            return Task.CompletedTask;
        }
    }
}
