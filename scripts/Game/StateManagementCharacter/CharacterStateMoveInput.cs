using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateMoveInput : BaseCharacterState, IStateObject<CharacterStateMoveInput.MovingOptions>
    {
        public struct MovingOptions
        {
            public CharacterController3D cc;
            public NavigationAgent3D agent;
            public InputActionBase[] actions;
        }

        MovingOptions _options;

        public BaseState GetState(MovingOptions options = default)
        {
            _options = options;
            return new BaseState(new() { OnUpdate = OnUpdate });
        }

        void OnUpdate()
        {
            var direction = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (direction != Vector2.Zero)
                _options.cc.Move(direction);
        }
    }
}
