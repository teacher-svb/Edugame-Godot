
using System;
using Godot;

namespace TnT.Systems.UI
{
    public partial class Notification : Control
    {
        [Export] Button _closeBtn;
        [Export] Button _nxtBtn;
        [Export] SubViewport _closeup;

        [Export] RichTextLabel _message;
        [Export] Label _characterName;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public string Text
        {
            get => _message.Text;
            set
            {
                _message.Text = value;
                _message.ResetSize();
            }
        }
        public string CharacterName { get => _characterName.Text; set => _characterName.Text = value; }
        public Camera3D CloseupCam { set => value.Reparent(_closeup); }

        public override async void _Ready()
        {
            _closeBtn.Pressed += () => CloseBtnPushed?.Invoke();
            _nxtBtn.Pressed += () => NextBtnPushed?.Invoke();
        }
    }
}