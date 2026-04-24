using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateIdle : BaseCharacterState, IStateObject<CharacterStateIdle.IdleOptions>
    {
        public struct IdleOptions { }

        CharacterStateManager _manager;

        public override void _Ready()
        {
            base._Ready();
            _manager = this.FindAncestorOfType<CharacterStateManager>();
        }

        public BaseState GetState(IdleOptions options = default)
        {
            return new BaseState(new() { OnEnter = OnEnter });
        }

        Task OnEnter()
        {
            _manager.PatrolAgent?.SetProcess(false);
            _manager.FollowAgent?.SetProcess(false);
            return Task.CompletedTask;
        }
    }
}
