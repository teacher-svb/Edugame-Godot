
using System;
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;

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

        public string CloseupTargetPath
        {
            set
            {
                if (_closeup.GetChildren().Count > 0)
                    _closeup.Clear();
                var fullScene = GD.Load<PackedScene>(value).Instantiate();
                var subtree = fullScene.FindAnyObjectByType<CharacterController3D>().VisualRoot;
                subtree.GetParent().RemoveChild(subtree);
                fullScene.QueueFree();
                _closeup.AddChild(subtree);
            }
        }

        public override async void _Ready()
        {
            _closeBtn.Pressed += () => CloseBtnPushed?.Invoke();
            _nxtBtn.Pressed += () => NextBtnPushed?.Invoke();
        }
    }
}