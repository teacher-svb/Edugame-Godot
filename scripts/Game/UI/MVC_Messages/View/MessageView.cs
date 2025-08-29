using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;
using TnT.Extensions;

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
            this.Modulate = Colors.Transparent;
            this.Scale = new Vector2(0, 0);

            _message.Text = "lorem ipsum";

            _message.NextBtnPushed += () => NextBtnPushed();
            _message.CloseBtnPushed += () => CloseBtnPushed();

            await Task.Yield();
        }

        public async Task ShowView(float duration = .2f)
        {
            var startColor = this.Modulate;
            var targetColor = Colors.White;
            var startScale = this.Scale;
            var targetScale = new Vector2(1, 1);

            await foreach (var t in Easings.Easings.Animate(duration, Ease.EaseOutCubic))
            {
                this.Modulate = startColor.Lerp(targetColor, t);
                this.Scale = startScale.Lerp(targetScale, t);
            }
        }

        public async Task HideView(float duration = .2f)
        {
            var startColor = this.Modulate;
            var targetColor = Colors.Transparent;
            var startScale = this.Scale;
            var targetScale = new Vector2(0, 0);

            await foreach (var t in Easings.Easings.Animate(duration, Ease.EaseOutCubic))
            {
                this.Modulate = startColor.Lerp(targetColor, t);
                this.Scale = startScale.Lerp(targetScale, t);
            }
        }

        public void SetMessage(string text, Texture2D sprite, string charName)
        {
            _message.Text = text;
            _message.CharacterSprite = sprite;
            _message.CharacterName = charName;
        }
    }

}
