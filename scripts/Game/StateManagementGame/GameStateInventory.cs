using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStateInventory : BaseGameState, IStateObject<GameStateInventory.InventoryOptions>
    {
        // public delegate void OnItemClicked(Item item);
        public delegate void OnCloseInventory();
        // [SerializeField] InputActionReference close;
        Camera2D camera;
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
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            // await InventoryController.Show();

            // close.action.Enable();
            var zoom = camera.Zoom;
            while (zoom.X > 1)
            {
                zoom.X -= .1f;
                zoom.Y -= .1f;
                await Task.Yield();
            }
        }

        async Task CloseInventory()
        {

            // await InventoryController.Hide();

            var zoom = camera.Zoom;
            while (zoom.X < 5)
            {
                zoom.X += .1f;
                zoom.Y += .1f;
                await Task.Yield();
            }
            // // await Inventory.Hide();
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;
        }

        bool ExitInventory()
        {
            // return close.action.triggered;
            return false;
        }
    }
}