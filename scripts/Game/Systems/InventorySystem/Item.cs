using Godot;
using TnT.Extensions;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class Item : Node3D
    {
        [Export] ItemData _staticData;
        [Export] ItemEventChannel _pickupChannel;

        public ItemData Data => _staticData;

        public void _OnBodyEntered(Node body)
        {
            if (body.FindAnyObjectByType<Player>() == null)
                return;

            _pickupChannel.Invoke(_staticData);
            GetParent().QueueFree();
        }
    }
}
