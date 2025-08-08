using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;

namespace TnT.EduGame.GameState
{
    [Serializable]
    public class GameStateInventory : IStateObject<GameStateInventory.InventoryOptions>, IGameState
    {
        // public delegate void OnItemClicked(Item item);
        public delegate void OnCloseInventory();
        // [SerializeField] InputActionReference close;
        // [SerializeField] CinemachineCamera camera;
        public struct InventoryOptions
        {
        }

        // InventoryController _inventoryController;

        // InventoryController InventoryController
        // {
        //     get
        //     {
        //         if (_inventoryController == null)
        //             _inventoryController = UnityEngine.Object.FindAnyObjectByType<InventoryController>();
        //         return _inventoryController;
        //     }
        // }

        // Inventory.Inventory _inventory;

        // Inventory.Inventory Inventory
        // {
        //     get
        //     {
        //         if (_inventory == null)
        //             _inventory = UnityEngine.Object.FindAnyObjectByType<Inventory.Inventory>();
        //         return _inventory;
        //     }
        // }

        public BaseState GetState(InventoryOptions options)
        {
            return new BaseState(new() { OnEnter = OpenInventory, ExitOnNextUpdate = ExitInventory, OnExit = CloseInventory });
        }

        async Task OpenInventory()
        {
            // ETime[play].timeScale = 0;
            // await InventoryController.Show();

            // close.action.Enable();

            // while (camera.Lens.OrthographicSize > 1)
            // {
            //     camera.Lens.OrthographicSize -= .1f;
            //     await Task.Yield();
            // }
        }

        async Task CloseInventory()
        {

            // await InventoryController.Hide();

            // while (camera.Lens.OrthographicSize < 5)
            // {
            //     camera.Lens.OrthographicSize += .1f;
            //     await Task.Yield();
            // }
            // // await Inventory.Hide();
            // ETime[play].timeScale = 1;
        }

        bool ExitInventory()
        {
            // return close.action.triggered;
            return false;
        }
    }
}