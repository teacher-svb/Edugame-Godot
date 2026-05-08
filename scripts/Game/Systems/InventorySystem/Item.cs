using System.Linq;
using Godot;
using TnT.EduGame.GameState;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class Item : Node3D
    {
        [Export] ItemData _staticData;
        [Export] ItemEventChannel _pickupChannel;
        Label3D _label;
        InputAction _interactAction => StateManagerGame.Instance.PickupItemAction;

        bool _playerInRange;

        public ItemData Data => _staticData;

        public override void _Ready()
        {
            _label = this.FindAnyObjectByType<Label3D>();
            var @event = InputMap.ActionGetEvents(_interactAction.ActionName).FirstOrDefault(e => e is InputEventKey) as InputEventKey;
            var key = DisplayServer.KeyboardGetKeycodeFromPhysical(@event.PhysicalKeycode);
            _label.Text = OS.GetKeycodeString(key);
        }

        public override void _Process(double delta)
        {
            if (_playerInRange && _interactAction.Triggered)
                Pickup();
        }

        public void _OnBodyEntered(Node body)
        {
            if (body.FindAnyObjectByType<Player>() == null)
                return;

            _playerInRange = true;
        }

        public void _OnBodyExited(Node body)
        {
            if (body.FindAnyObjectByType<Player>() == null)
                return;

            _playerInRange = false;
        }

        private void Pickup()
        {
            _pickupChannel.Invoke(_staticData);
            GetParent().QueueFree();
        }
    }
}
