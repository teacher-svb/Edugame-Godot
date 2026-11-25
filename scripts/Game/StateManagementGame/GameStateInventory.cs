using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
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
            return new BaseState(new() { OnEnter = Open, OnResume = Resume, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.close.Triggered)
                _closed = true;
        }

        private bool Exit() => _closed;

        private async Task Open()
        {
            await InventoryController.Instance.Show();
            _options.close.Enable();
        }

        // a state on top of it (e.g. a message) closed and disabled the shared close action
        private Task Resume()
        {
            _options.close.Enable();
            return Task.CompletedTask;
        }

        private async Task Close()
        {
            _options.close.Disable();
            await InventoryController.Instance.Hide();
        }
    }
}
