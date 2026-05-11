using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStateSettings : BaseGameState, IStateObject<GameStateSettings.Options>
    {
        public struct Options
        {
            public InputAction close;
            public bool showAudio;
            public bool showDisplay;
            public bool showControls;
            public bool showAccessibility;
        }

        private bool _closed;
        private Options _options;

        public BaseState GetState(Options options)
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
            _options.close.Enable();
            SettingsController.Instance.OnBack += HandleBack;
            await SettingsController.Instance.Show(
                _options.showAudio,
                _options.showDisplay,
                _options.showControls,
                _options.showAccessibility
            );
        }

        private async Task Close()
        {
            SettingsController.Instance.OnBack -= HandleBack;
            _options.close.Disable();
            await SettingsController.Instance.Hide();
        }

        private void HandleBack() => _closed = true;
    }
}
