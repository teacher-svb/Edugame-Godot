using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.State;

namespace TnT.EduGame.Characters.States
{
    [GlobalClass]
    public partial class CharacterStateIdle : BaseCharacterState, IStateObject<CharacterStateIdle.IdleOptions>
    {
        public struct IdleOptions
        {
            /// <summary>How long to idle; 0 ends at once, a negative value never ends (the base state).</summary>
            public int durationMs;
            /// <summary>Called once, when the state leaves the stack (after its time, or popped / unwound earlier).</summary>
            public Action ended;
        }

        public BaseState GetState(IdleOptions options = default)
        {
            var duration = options.durationMs;
            bool over = duration == 0;

            return new BaseState(new()
            {
                ExitOnNextUpdate = () => over,
                OnEnter = () =>
                {
                    // a tree timer that stops while the game is paused (Task.Delay ran on through a dialogue or the
                    // pause menu); not awaited here, so the stack isn't stuck in this state's enter while it idles
                    if (duration > 0)
                        GetTree().CreateTimer(duration / 1000.0, processAlways: false).Timeout += () => over = true;
                    return Task.CompletedTask;
                },
                OnExit = () =>
                {
                    options.ended?.Invoke();
                    return Task.CompletedTask;
                },
            });
        }
    }
}
