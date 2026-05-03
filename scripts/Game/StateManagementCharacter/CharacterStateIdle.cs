using System;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateIdle : BaseCharacterState, IStateObject<CharacterStateIdle.IdleOptions>
    {
        private IdleOptions _options;

        public struct IdleOptions
        {
            public int durationMs;
        }

        public BaseState GetState(IdleOptions options = default)
        {
            _options = options;
            return new BaseState(new() { ExitOnNextUpdate = Stop, OnEnter = OnEnter });
        }

        private bool Stop()
        {
            return _options.durationMs == 0;
        }

        private async Task OnEnter()
        {
            if (_options.durationMs >= 0)
            {
                await Task.Delay(_options.durationMs);
                _options.durationMs = 0;
            }
        }
    }
}
