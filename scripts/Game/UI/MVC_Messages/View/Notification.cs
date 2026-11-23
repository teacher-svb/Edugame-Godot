
using System;
using System.Linq;
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
        [Export] DirectionalLight3D _closeupLight;
        Node3D _closeupModel;

        // Key light relative to the closeup camera: from above and slightly to the
        // camera's left, so the face the camera sees is always lit.
        static readonly Basis KeyLightOffset =
            new Basis(Vector3.Up, Mathf.DegToRad(-30f)) * new Basis(Vector3.Right, Mathf.DegToRad(-35f));

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
                // The SubViewport has its own World3D (own_world_3d), so the copied model
                // is not added to the level. Only the previous model is removed: the
                // viewport's light stays.
                if (_closeupModel != null && IsInstanceValid(_closeupModel))
                {
                    _closeup.RemoveChild(_closeupModel);
                    _closeupModel.QueueFree();
                }
                var fullScene = GD.Load<PackedScene>(value).Instantiate();
                var subtree = fullScene.FindAnyObjectByType<CharacterController3D>().VisualRoot;
                subtree.GetParent().RemoveChild(subtree);
                fullScene.QueueFree();
                _closeup.AddChild(subtree);
                _closeupModel = subtree;
                if (subtree.FindChildren("*", nameof(Camera3D), true, false).FirstOrDefault() is Camera3D cam)
                {
                    cam.MakeCurrent();
                    if (_closeupLight != null)
                        _closeupLight.GlobalBasis = cam.GlobalBasis.Orthonormalized() * KeyLightOffset;
                }
            }
        }

        public override async void _Ready()
        {
            _closeBtn.Pressed += () => CloseBtnPushed?.Invoke();
            _nxtBtn.Pressed += () => NextBtnPushed?.Invoke();
        }
    }
}