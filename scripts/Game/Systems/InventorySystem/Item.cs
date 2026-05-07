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

        public bool IsPersistent => _staticData.IsPersistent;
        public string ItemId => _staticData.Id;
        public Texture2D Icon => _staticData.Icon;

        public ItemData Data => _staticData;
    }
}