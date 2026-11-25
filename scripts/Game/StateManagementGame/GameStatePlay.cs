using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.Input;

namespace TnT.EduGame.GameStates
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
            public InputAction pauseGame;
            public PlayerTutorial tutorial;
        }

        public BaseState GetState(PlayOptions options)
        {
            _options = options;
            return new BaseState(new() { OnEnter = StartGame, OnResume = ResumeGame, OnExit = StopGame, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.openInventory.Triggered)
                StateManagerGame.Instance.OpenInventory();
            else if (_options.pauseGame.Triggered)
            {
                StateManagerGame.Instance.OpenPauseMenu();
            }
        }

        Task StartGame()
        {
            if (_options.tutorial != null)
                _options.tutorial.MoveAction = _options.move;
            return ResumeGame();
        }

        // also after a state on top of it closed: the game runs again and the play input is on (a state above may
        // have paused the tree, e.g. the dev menu)
        Task ResumeGame()
        {
            ManagerUI.Instance.GetTree().Paused = false;
            SetInput(true);
            return Task.CompletedTask;
        }

        // popped when a scene load unwinds the stack: no gameplay input until the next level's play state
        Task StopGame()
        {
            SetInput(false);
            return Task.CompletedTask;
        }

        void SetInput(bool enabled)
        {
            foreach (InputActionBase action in new InputActionBase[] { _options.openInventory, _options.pauseGame, _options.move, _options.jump })
            {
                if (action == null)
                    continue;
                if (enabled)
                    action.Enable();
                else
                    action.Disable();
            }
        }
    }
}