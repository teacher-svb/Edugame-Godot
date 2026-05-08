using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.Input;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStatePlay : BaseGameState, IStateObject<GameStatePlay.PlayOptions>
    {
        private PlayOptions _options;

        public struct PlayOptions
        {
            public InputAction jump;
            public InputAction2D move;
            public InputAction openInventory;
            public InputAction pickupItem;
        }

        public BaseState GetState(PlayOptions options)
        {
            _options = options;
            return new BaseState(new() { OnEnter = StartGame, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.openInventory.Triggered)
            {
                StateManagerGame.Instance.OpenInventory();
            }
        }

        async Task StartGame()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;

            _options.openInventory.Enable();
            
            await Task.Yield();
        }
    }
}