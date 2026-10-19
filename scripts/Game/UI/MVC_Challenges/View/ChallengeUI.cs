

using System;
using System.Linq;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // Three slots: visualization (left), question text (right), input (bottom centre).
    [GlobalClass]
    public partial class ChallengeUI : Control
    {
        [Signal]
        public delegate void OnSubmitEventHandler();
        [Signal]
        public delegate void OnValueAssignedEventHandler(string paramName, int value);
        [Signal]
        public delegate void OnValueClearedEventHandler(string paramName);

        [Export]
        Control _visualizationContainer;
        [Export]
        Control _questionContainer;
        [Export]
        Control _inputContainer;
        [Export]
        Control _submitContainer;

        Button _submitButton;

        public override void _Ready()
        {
            _submitButton = _submitContainer.FindObjectsByType<Button>().First();
            _submitButton.Pressed += () => EmitSignal(SignalName.OnSubmit);
        }

        public void SetSubmitEnabled(bool enabled) => _submitButton.Disabled = !enabled;

        public class Builder
        {
            private readonly ChallengeUI _ui;
            private readonly IMathChallenge _challenge;
            private readonly ChallengeRound _round;

            // set once a widget in any slot takes care of player input
            private bool _hasInput;

            public Builder(IMathChallenge challenge, ChallengeRound round)
            {
                _challenge = challenge;
                _round = round;

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                _ui = tree.FindAnyObjectByType<ChallengeUI>();
            }

            public ChallengeUI Build() => _ui;

            public Builder WithQuestion()
            {
                _ui._questionContainer.Clear();

                var label = _ui._questionContainer.CreateChild<RichTextLabel>();
                label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                label.SizeFlagsVertical = SizeFlags.ExpandFill;
                label.SetAnchorsPreset(LayoutPreset.FullRect);
                label.Text = _challenge.QuestionFor(_round);

                return this;
            }

            // A challenge without a visualization is fine: the question text alone carries it.
            public Builder WithVisualization()
            {
                _ui._visualizationContainer.Clear();

                if (_challenge.Visualization == null)
                    return this;

                var widget = Instantiate<ChallengeVisualizationWidget>(_challenge.Visualization.ResolveScene(), _challenge.Visualization);
                widget.Init(_challenge, _round);
                if (widget is IChallengeInput input)
                    WireInput(input);
                _ui._visualizationContainer.AddChild(widget);

                return this;
            }

            // Call after WithVisualization, so a visualization that doubles as input wins.
            public Builder WithInput()
            {
                _ui._inputContainer.Clear();

                if (_hasInput)
                {
                    if (_challenge.Input != null)
                        GD.PushWarning($"{_challenge.Name}: visualization already handles input, ignoring {_challenge.Input.GetType().Name}");
                    return this;
                }

                if (_challenge.Input == null)
                    throw new InvalidOperationException($"{_challenge.Name}: no Input assigned and the visualization doesn't handle input");

                var widget = Instantiate<ChallengeInputWidget>(_challenge.Input.ResolveScene(), _challenge.Input);
                widget.Init(_challenge, _round);
                WireInput(widget);
                _ui._inputContainer.AddChild(widget);

                return this;
            }

            void WireInput(IChallengeInput input)
            {
                input.ValueAssigned += (paramName, value) => _ui.EmitSignal(SignalName.OnValueAssigned, paramName, value);
                input.ValueCleared += paramName => _ui.EmitSignal(SignalName.OnValueCleared, paramName);
                _hasInput = true;
            }

            static T Instantiate<T>(PackedScene scene, Resource owner) where T : Control =>
                scene?.Instantiate() as T
                    ?? throw new InvalidOperationException($"{owner.GetType().Name}: scene is missing or its root is not a {typeof(T).Name}");
        }
    }
}
