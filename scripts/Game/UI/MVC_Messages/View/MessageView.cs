using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class MessageView : Control
    {
        // Control this;
        // Control container;

        Notification _message;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public async Task InitializeView()
        {
            // this.Clear();

            // container = this.CreateChild<Control>("container");

            _message = this.FindAnyObjectByType<Notification>();
            this.Modulate = Colors.Transparent;
            this.Scale = new Vector2(0, 0);

            // _message = container.CreateChild<Notification>("dialog");
            _message.Text = "lorem ipsum";

            _message.NextBtnPushed += () => NextBtnPushed();
            _message.CloseBtnPushed += () => CloseBtnPushed();

            await Task.Yield();
        }

        public async Task ShowView()
        {
            // container.RemoveFromClassList("warning");
            // container.RemoveFromClassList("error");
            // container.AddToClassList("opened");


            var steps = 100;

            var color = Colors.White;
            var scale = new Vector2(1, 1);

            for (float i = 0; i < 1; i += 1f / steps)
            {
                await Task.Delay(1000 / steps);
                this.Modulate = this.Modulate.Lerp(color, i);
                this.Scale = this.Scale.Lerp(scale, i);
            }
        }

        public async Task HideView()
        {
            // container.RemoveFromClassList("opened");
            var steps = 100;

            var color = Colors.Transparent;
            var scale = new Vector2(0.1f,0.1f);

            for (float i = 0; i < 1; i += 1f / steps)
            {
                await Task.Delay(1000 / steps);
                this.Modulate = this.Modulate.Lerp(color, i);
                this.Scale = this.Scale.Lerp(scale, i);
            }
        }

        public void SetMessage(string text, Texture2D sprite, string charName)
        {
            _message.Text = text;
            // _message.CharacterSprite = sprite;
            // _message.CharacterName = charName;
        }
    }

}
