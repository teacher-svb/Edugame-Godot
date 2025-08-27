
using System;
using Godot;

namespace TnT.Systems.UI
{
    public partial class Notification : Control
    {
        [Export] Button _closeBtn;
        [Export] Button _nxtBtn;
        [Export] TextureRect _characterSprite;

        [Export] RichTextLabel _message;
        [Export] Label _characterName;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public string Text { get => _message.Text; set => _message.Text = value; }
        public string CharacterName { get => _characterName.Text; set => _characterName.Text = value; }
        public Texture2D CharacterSprite { set => _characterSprite.Texture = value; }

        public override void _Ready()
        {
            _closeBtn.Pressed += () => CloseBtnPushed();
            _nxtBtn.Pressed += () => NextBtnPushed();
        }
    }
}