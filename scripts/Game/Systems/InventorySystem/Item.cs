using System;
using Godot;

namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// Represents an in-game Item, based on an ItemData Resource.
    /// </summary>
    [GlobalClass]
    public partial class Item : Node2D
    {
        public Action<Item> OnItemUsed;

        [Export]
        ItemData _staticData;
        [Export]
        string _id = Guid.NewGuid().ToString();
        public string Id => _id;

        public bool IsPersistent { get => _staticData.IsPersistent; private set => _staticData.IsPersistent = value; }
        public string ItemId { get => _staticData.Id; private set => _staticData.Id = value; }
        public Texture2D Icon { get => _staticData.Icon; private set => _staticData.Icon = value; }

        public ItemData Data => _staticData;
    }
}