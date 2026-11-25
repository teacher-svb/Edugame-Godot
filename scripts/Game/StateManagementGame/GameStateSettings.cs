using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
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
            /// <summary>Runs once the screen is closed (Back, Escape, or unwound by a scene load).</summary>
            public System.Action onClosed;
        }

        private bool _closed;
        private Options _options;

        public BaseState GetState(Options options)
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
            _options.close.Enable();
            SettingsController.Instance.OnBack += HandleBack;
            await SettingsController.Instance.Show(
                _options.showAudio,
                _options.showDisplay,
                _options.showControls,
                _options.showAccessibility
            );
        }

        // a state on top of it closed (and disabled the shared close action)
        private Task Resume()
        {
            _options.close.Enable();
            return Task.CompletedTask;
        }

        private async Task Close()
        {
            SettingsController.Instance.OnBack -= HandleBack;
            _options.close.Disable();
            await SettingsController.Instance.Hide();
            _options.onClosed?.Invoke();
        }

        private void HandleBack() => _closed = true;
    }
}
