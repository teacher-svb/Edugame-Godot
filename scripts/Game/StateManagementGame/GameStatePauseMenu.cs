using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStatePauseMenu : BaseGameState, IStateObject<GameStatePauseMenu.PauseMenu>
    {
        public struct PauseMenu
        {
            public InputAction close;
        }

        private bool _closed;
        private PauseMenu _options;

        public BaseState GetState(PauseMenu options)
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
            ManagerUI.Instance.GetTree().Paused = true;
            _options.close.Enable();
            PauseMenuController.Instance.OnResume   += HandleResume;
            PauseMenuController.Instance.OnSettings += HandleSettings;
            PauseMenuController.Instance.OnSave     += HandleSave;
            PauseMenuController.Instance.OnLoad     += HandleLoad;
            await PauseMenuController.Instance.Show();
        }

        private async Task Close()
        {
            PauseMenuController.Instance.OnResume   -= HandleResume;
            PauseMenuController.Instance.OnSettings -= HandleSettings;
            PauseMenuController.Instance.OnSave     -= HandleSave;
            PauseMenuController.Instance.OnLoad     -= HandleLoad;
            _options.close.Disable();
            await PauseMenuController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
        }

        private void HandleResume() => _closed = true;

        private void HandleSettings() =>
            StateManagerGame.Instance.OpenSettings(audio: true, display: false, controls: false, accessibility: true);

        private void HandleSave() => SaveLoadManager.Instance.SaveGame();

        private void HandleLoad()
        {
            // ResetStack() will wipe this state without calling Close() — clean up manually
            PauseMenuController.Instance.OnResume   -= HandleResume;
            PauseMenuController.Instance.OnSettings -= HandleSettings;
            PauseMenuController.Instance.OnSave     -= HandleSave;
            PauseMenuController.Instance.OnLoad     -= HandleLoad;
            _options.close.Disable();
            ManagerUI.Instance.GetTree().Paused = false;
            SaveLoadManager.Instance.ReloadGame();
        }
    }
}
