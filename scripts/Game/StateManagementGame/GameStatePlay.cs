using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.Input;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStatePlay : BaseGameState, IStateObject<GameStatePlay.PlayOptions>, IInputActionable
    {
        private PlayOptions _options;

        public InputActionBase[] InputActions => [];

        public struct PlayOptions
        {
            public InputAction jump;
            public InputAction2D move;
            public InputAction openInventory;
        }
        // [SerializeField] InputActionReference openInventory;

        // ActionsMenuController _actionsMenuController;

        // ActionsMenuController ActionsMenuController
        // {
        //     get {
        //         if (_actionsMenuController == null)
        //             _actionsMenuController = UnityEngine.Object.FindAnyObjectByType<ActionsMenuController>();
        //         return _actionsMenuController;
        //     }
        // }

        public BaseState GetState(PlayOptions options)
        {
            _options = options;
            return new BaseState(new() { OnEnter = StartGame, OnUpdate = Update });
        }

        private void Update()
        {
            // if (_options.openInventory.Triggered)
            // {
            //     StateManagerGame.Instance.OpenInventory();
            // }
        }

        // private void OnOpenInventory(InputAction.CallbackContext context) => StateManagerGame.Instance.OpenInventory();
        async Task StartGame()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;

            // openInventory.action.Enable();
            
            // openInventory.action.performed += OnOpenInventory;
            await Task.Yield();
        }
    }
}