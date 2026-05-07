using System;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class ItemEventListener : EventListener
    {
        public Action<ItemData> OnItemEvent;

        public override void Raise(params Variant[] values)
        {
            if (values.Length > 0)
                OnItemEvent?.Invoke(values[0].As<ItemData>());
        }
    }
}
