using System;
using Godot;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class ItemData : Resource
    {
        [Export]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Export]
        public string Name;
        [Export]
        public Texture2D Icon;
        [Export(PropertyHint.MultilineText)]
        public string Description;
        [Export]
        public bool IsPersistent = false;
    }
}
