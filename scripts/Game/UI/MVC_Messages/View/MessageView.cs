using System;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class MessageView : Control
    {
        Notification _message;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public async Task InitializeView()
        {
            _message = this.FindAnyObjectByType<Notification>();
            Modulate = Colors.Transparent;
            Scale = Vector2.Zero;

            _message.Text = "lorem ipsum";

            _message.NextBtnPushed += () => NextBtnPushed?.Invoke();
            _message.CloseBtnPushed += () => CloseBtnPushed?.Invoke();

            await Task.Yield();
        }

        public async Task ShowView(float duration = 0.2f) => await this.ScaleIn(duration);

        public async Task HideView(float duration = 0.2f) => await this.ScaleOut(duration);

        public void SetMessage(string text, Texture2D sprite, string charName)
        {
            _message.Text = text;
            _message.CharacterSprite = sprite;
            _message.CharacterName = charName;
        }
    }
}
