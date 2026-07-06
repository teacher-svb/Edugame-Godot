using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
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
        }

        MovingOptions _options;

        public BaseState GetState(MovingOptions options = default)
        {
            _options = options;
            return new BaseState(new() { OnEnter = OnEnter, OnExit = OnExit, OnUpdate = OnUpdate });
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

        private async Task OnExit()
        {
        }

        private void OnHeld(InputActionBase action)
        {
            var dir = (action as InputAction2D).Value;
            if (dir != Vector2.Zero)
                _options.cc.Move(dir);
        }
    }
}
