
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ActionsMenuModel : Resource
    {
        [Signal]
        public delegate void OnButtonClickedEventHandler();
        [GlobalClass]
        public partial class ButtonData : Resource
        {
            [Export] public string buttonLabel = "";
            public OnButtonClickedEventHandler OnButtonClicked;
        }

        public List<ButtonData> Buttons = new List<ButtonData>();

        public void Add(ButtonData item)
        {
            Buttons.Add(item);
        }

        public ButtonData Get(int index) => Buttons.ElementAtOrDefault(index);
        public void Clear() => Buttons.Clear();
    }
}
