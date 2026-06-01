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
        public struct IdleOptions
        {
            public int durationMs;
        }

        public BaseState GetState(IdleOptions options = default)
        {
            var duration = options.durationMs;

            return new BaseState(new()
            {
                ExitOnNextUpdate = () => duration == 0,
                OnEnter = async () =>
                {
                    if (duration < 0)
                        return;
                    await Task.Delay(duration);
                    duration = 0;
                }
            });
        }
    }
}
