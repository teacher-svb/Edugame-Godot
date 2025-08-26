using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class MessageView : Resource
    {
        Control _root;
        Control container;

        Notification _message;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public async Task InitializeView(Control root)
        {
            _root = root;
            _root.Clear();

            container = _root.CreateChild<Control>("container");

            _message = container.CreateChild<Notification>("dialog");
            _message.Text = "lorem ipsum";

            _message.NextBtnPushed += () => NextBtnPushed();
            _message.CloseBtnPushed += () => CloseBtnPushed();

            await Task.Yield();
        }

        public void Show()
        {
            // container.RemoveFromClassList("warning");
            // container.RemoveFromClassList("error");
            // container.AddToClassList("opened");
        }

        public void ShowWarning()
        {
            Show();
            // container.AddToClassList("warning");
        }

        public void ShowError()
        {
            Show();
            // container.AddToClassList("error");
        }

        public void Hide()
        {
            // container.RemoveFromClassList("opened");
        }

        public void SetMessage(string text, Texture2D sprite, string charName)
        {
            _message.Text = text;
            _message.CharacterSprite = sprite;
            _message.CharacterName = charName;
        }
    }

}
