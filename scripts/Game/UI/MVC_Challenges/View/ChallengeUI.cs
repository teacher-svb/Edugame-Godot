

using System;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeUI : Control
    {
        [Signal]
        public delegate void OnSubmitEventHandler();
        [Signal]
        public delegate void OnValueAssignedEventHandler(string paramName, int value);

        [Export]
        Control _left;
        [Export]
        Control _right;
        [Export]
        Control _challengeContainer;
        [Export]
        Control _questionContainer;
        [Export]
        Control _submitContainer;

        [Export]
        Dictionary<ChallengeUIType, Resource> _challengeScenes;

        Button _submitButton;

        public override void _Ready()
        {
            _submitButton = _submitContainer.FindObjectsByType<Button>().First();
            _submitButton.Pressed += () => EmitSignal(SignalName.OnSubmit);
        }

        public void SetSubmitEnabled(bool enabled) => _submitButton.Disabled = !enabled;

        public class Builder
        {
            private readonly Control _challengeContainer;
            private readonly Control _questionContainer;
            private readonly ChallengeUI _ui;
            private readonly IMathChallenge _challenge;

            public Builder(IMathChallenge challenge)
            {
                _challenge = challenge;

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                _ui = tree.FindAnyObjectByType<ChallengeUI>();

                _challengeContainer = _ui._challengeContainer;
                _questionContainer = _ui._questionContainer;
            }

            public ChallengeUI Build() => _ui;

            public Builder WithQuestionElement()
            {
                _questionContainer.Clear();

                var label = _questionContainer.CreateChild<RichTextLabel>();
                label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                label.SizeFlagsVertical = SizeFlags.ExpandFill;
                label.SetAnchorsPreset(LayoutPreset.FullRect);
                label.Text = _challenge.Question;

                return this;
            }

            // The one seam every challenge visualization goes through: instance
            // the scene matched to this challenge's ChallengeUIType (wired in the
            // _challengeScenes export), hand it the challenge, and drop it in the
            // challenge container. Nothing upstream needs to know whether that
            // scene is a spinbox, a set of cogwheels, or a hand-authored subscene
            // full of custom controls.
            public Builder WithSceneWidget()
            {
                _challengeContainer.Clear();

                if (!_ui._challengeScenes.TryGetValue(_challenge.ChallengeUIType, out var resource) || resource is not PackedScene scene)
                    throw new InvalidOperationException($"No widget scene assigned for {_challenge.ChallengeUIType} in ChallengeUI._challengeScenes");

                var widget = scene.Instantiate<ChallengeInputWidget>();
                widget.Init(_challenge);
                widget.ValueAssigned += (paramName, value) => _ui.EmitSignal(SignalName.OnValueAssigned, paramName, value);
                _challengeContainer.AddChild(widget);

                return this;
            }
        }
    }
}
