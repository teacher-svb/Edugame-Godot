using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateMoving : BaseCharacterState, IStateObject<CharacterStateMoving.MovingOptions>
    {
        public struct MovingOptions { }

        CharacterStateManager _manager;

        public override void _Ready()
        {
            base._Ready();
            _manager = this.FindAncestorOfType<CharacterStateManager>();
        }

        public BaseState GetState(MovingOptions options = default)
        {
            return new BaseState(new() { OnEnter = OnEnter, OnUpdate = OnUpdate });
        }

        Task OnEnter()
        {
            _manager.PatrolAgent?.SetProcess(false);
            _manager.FollowAgent?.SetProcess(false);
            return Task.CompletedTask;
        }

        void OnUpdate()
        {
            var direction = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (direction != Vector2.Zero)
                _manager.Controller.Move(direction);
        }
    }
}
