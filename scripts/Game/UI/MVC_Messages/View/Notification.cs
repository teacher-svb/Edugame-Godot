
using System;
using System.Linq;
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;

namespace TnT.EduGame.UI
{
    public partial class Notification : Control
    {
        [Export] Button _closeBtn;
        [Export] Button _nxtBtn;
        [Export] SubViewport _closeup;
        [Export] DirectionalLight3D _closeupLight;
        Node3D _closeupModel;
        string _closeupModelPath;   // the character scene _closeupModel was copied from (reused for the next line)

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
        public string CharacterName
        {
            get => _characterName.Text;
            set
            {
                _characterName.Text = value ?? "";
                _characterName.Visible = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// The speaker's character scene for the closeup. Empty, or a scene without a usable model: no closeup (the
        /// viewport is hidden). The same scene as the line before keeps its model instead of instantiating it again.
        /// </summary>
        public string CloseupTargetPath
        {
            set
            {
                bool shown = !string.IsNullOrEmpty(value)
                    && (value == _closeupModelPath && _closeupModel != null && IsInstanceValid(_closeupModel)
                        || ReplaceCloseupModel(value));
                if (_closeup.GetParent() is Control container)
                    container.Visible = shown;
            }
        }

        bool ReplaceCloseupModel(string scenePath)
        {
            // The SubViewport has its own World3D (own_world_3d), so the copied model
            // is not added to the level. Only the previous model is removed: the
            // viewport's light stays.
            if (_closeupModel != null && IsInstanceValid(_closeupModel))
            {
                _closeup.RemoveChild(_closeupModel);
                _closeupModel.QueueFree();
            }
            _closeupModel = null;
            _closeupModelPath = null;

            var packed = ResourceLoader.Exists(scenePath) ? ResourceLoader.Load(scenePath) as PackedScene : null;
            if (packed == null)
            {
                GD.PushWarning($"Notification: no character scene at '{scenePath}', no closeup");
                return false;
            }
            var fullScene = packed.Instantiate();
            var subtree = fullScene.FindAnyObjectByType<CharacterController3D>(includeSelf: true)?.VisualRoot;
            if (subtree == null)
            {
                GD.PushWarning($"Notification: '{scenePath}' has no CharacterController3D with a VisualRoot, no closeup");
                fullScene.Free();
                return false;
            }
            subtree.GetParent().RemoveChild(subtree);
            subtree.Owner = null;   // owned by the scene it came from (Godot warns when it is added elsewhere)
            fullScene.QueueFree();
            _closeup.AddChild(subtree);
            _closeupModel = subtree;
            _closeupModelPath = scenePath;
            if (subtree.FindChildren("*", nameof(Camera3D), true, false).FirstOrDefault() is Camera3D cam)
            {
                cam.MakeCurrent();
                if (_closeupLight != null)
                    _closeupLight.GlobalBasis = cam.GlobalBasis.Orthonormalized() * KeyLightOffset;
            }
            return true;
        }

        public override void _Ready()
        {
            // mouse only: a clicked button kept the keyboard focus (the box is only scaled away, never hidden), so a
            // later Space (`next`, also ui_accept) pressed it as well and closed the next box at its first line
            _closeBtn.FocusMode = FocusModeEnum.None;
            _nxtBtn.FocusMode = FocusModeEnum.None;
            _closeBtn.Pressed += () => CloseBtnPushed?.Invoke();
            _nxtBtn.Pressed += () => NextBtnPushed?.Invoke();
        }
    }
}