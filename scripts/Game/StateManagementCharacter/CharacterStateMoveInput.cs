using System;
using System.Threading.Tasks;
using Godot;
using TnT.Input;
using TnT.Systems.State;

namespace TnT.EduGame.Characters.States
{
    [GlobalClass]
    public partial class CharacterStateMoveInput :
        BaseCharacterState,
        IStateObject<CharacterStateMoveInput.MovingOptions>
    {
        public struct MovingOptions
        {
            public CharacterController3D cc;
            public NavigationAgent3D agent;
            public InputAction2D moveAction;
            public InputAction jumpAction;
            /// <summary>Called once, when the state leaves the stack.</summary>
            public Action ended;
        }

        MovingOptions _options;

        public BaseState GetState(MovingOptions options = default)
        {
            _options = options;
            return new BaseState(new()
            {
                OnEnter = OnEnter,
                OnUpdate = OnUpdate,
                OnExit = () =>
                {
                    options.ended?.Invoke();   // this push's callback, not the field's (set again by the next push)
                    return Task.CompletedTask;
                },
            });
        }

        private void OnUpdate()
        {
            if (_options.jumpAction.Triggered)
            {
                _options.cc.Jump();
            }
            if (_options.moveAction.IsPressed)
            {
                var dir = _options.moveAction.Value;
                if (dir != Vector2.Zero)
                    _options.cc.Move(dir);
            }
        }

        private async Task OnEnter()
        {
            if (_options.jumpAction.IsPressed)
                _options.cc.Jump();
        }
    }
}
