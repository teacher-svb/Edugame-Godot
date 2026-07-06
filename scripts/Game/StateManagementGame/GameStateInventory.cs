using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStateInventory : BaseGameState, IStateObject<GameStateInventory.InventoryOptions>
    {
        public struct InventoryOptions
        {
            public InputAction close;
        }

        private bool _closed;
        private InventoryOptions _options;

        public BaseState GetState(InventoryOptions options)
        {
            _options = options;
            _closed = false;
            return new BaseState(new() { OnEnter = Open, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.close.Triggered)
                _closed = true;
        }

        private bool Exit() => _closed;

        private async Task Open()
        {
            // ManagerUI.Instance.GetTree().Paused = true;
            await InventoryController.Instance.Show();
            _options.close.Enable();
        }

        private async Task Close()
        {
            _options.close.Disable();
            await InventoryController.Instance.Hide();
            // ManagerUI.Instance.GetTree().Paused = false;
        }
    }
}
