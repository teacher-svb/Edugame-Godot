using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
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
            return new BaseState(new() { OnEnter = Open, OnPause = Pause, OnResume = Resume, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.close.Triggered)
                _closed = true;
        }

        private bool Exit() => _closed;

        // once per push: back from Settings is a resume, not a second Open (the handlers would double up)
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

        // Settings opened on top: Space/Enter must not press the menu's focused button behind it
        private Task Pause()
        {
            PauseMenuController.Instance.Pause();
            return Task.CompletedTask;
        }

        // back from Settings: its Close disabled the shared close action
        private Task Resume()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            _options.close.Enable();
            PauseMenuController.Instance.Resume();
            return Task.CompletedTask;
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

        private void HandleSave()
        {
            SaveLoadManager.Instance.SaveGame();
            PauseMenuController.Instance.RefreshLoadEnabled();   // the first save enables "Laden"
        }

        // the loader unwinds the stack below itself before it changes the scene, so this state's Close runs then
        // (handlers off, menu hidden); nothing to clean up here. No (readable) save: ReloadGame logs and returns
        // false without changing anything, the menu stays open.
        private void HandleLoad() => SaveLoadManager.Instance.ReloadGame();
    }
}
