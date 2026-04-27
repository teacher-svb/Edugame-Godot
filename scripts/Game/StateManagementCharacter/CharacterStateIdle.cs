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
        public struct IdleOptions { }

        public BaseState GetState(IdleOptions options = default)
        {
            return new BaseState(new() { ExitOnNextUpdate = Exit, OnEnter = OnEnter, OnExit = OnExit });
        }

        private bool Exit()
        {
            return false;
        }

        private async Task OnEnter()
        {
            GD.Print("entering idle state");
        }

        private async Task OnExit()
        {
            GD.Print("leaving idle state");
        }
    }
}
