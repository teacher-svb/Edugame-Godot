using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;

namespace TnT.EduGame.GameState
{
    [Serializable]
    public class GameStatePlay : IStateObject<GameStatePlay.PlayOptions>, IGameState
    { 
        public struct PlayOptions
        {
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

        public BaseState GetState(PlayOptions options) => new BaseState(new() { OnEnter = StartGame, OnUpdate = Update });

        private void Update()
        {
            // if (openInventory.action.triggered)
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