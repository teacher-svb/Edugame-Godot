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
            return new BaseState(new() { ExitOnNextUpdate = Exit });
        }

        private bool Exit()
        {
            return false;
        }
    }
}
