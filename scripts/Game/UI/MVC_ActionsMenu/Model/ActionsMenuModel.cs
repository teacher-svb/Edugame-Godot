
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ActionsMenuModel : Resource
    {
        public List<ButtonData> Buttons = new List<ButtonData>();

        public void Add(ButtonData item)
        {
            Buttons.Add(item);
        }

        public ButtonData Get(int index) => Buttons.ElementAtOrDefault(index);
        public void Clear() => Buttons.Clear();
    }
}
