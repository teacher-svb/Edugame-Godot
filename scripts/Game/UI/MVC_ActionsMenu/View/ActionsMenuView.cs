using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ActionsMenuView : Resource
    {
        Control _root;
        Control container;

        public async Task InitializeView(Control root)
        {
            _root = root;
            root.Clear();

            container = root.CreateChild<Control>("container");


            await Task.Yield();
        }

        public void AddButton(string label, Action btnClick)
        {
            var btn = container.CreateChild<Button>(label);
            btn.Text = label;

            btn.Pressed += btnClick;
        }

        public void Clear() {
            container.Clear();
        }

        public void Show()
        {
            _root.Visible = true;
        }

        public void Hide()
        {
            _root.Visible = false;
        }
    }
}
