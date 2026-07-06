using System;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class ItemEventListener : EventListener
    {
        [Export] bool _checkItemId;
        [Export] string _itemId;
        public Action<ItemData> OnItemEvent;

        public override void Raise(params Variant[] values)
        {
            // if (values.Length > 0)
            // {
            //     var item = values[0].As<ItemData>();
            //     GD.Print(item.Id);
            //     if (_checkItemId == false || _itemId == item.Id)
            //         OnItemEvent?.Invoke(values[0].As<ItemData>());
            // }


            if (values?[0].Obj is ItemData item)
            {
                if (_checkItemId && item.Id != _itemId) return;
                base.Raise(item);
            }
        }
    }
}
