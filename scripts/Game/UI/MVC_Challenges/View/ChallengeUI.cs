

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
        public delegate void OnValueSelectedEventHandler(int valueIndex);
        [Signal]
        public delegate void OnValueChangedEventHandler(string name, string value);

        [Export]
        Control _left;
        [Export]
        Control _right;
        [Export]
        Control _challengeContainer;
        [Export]
        Control _questionContainer;
        [Export]
        Control _answerContainer;
        [Export]
        Control _submitContainer;
        [Export]
        SpinBox _answerSpinbox;

        [Export]
        Dictionary<ChallengeUIType, Resource> _challengeScenes;

        // Persistent forwarders: _submitContainer/_answerSpinbox are static scene furniture,
        // never freed/recreated between challenges (unlike the containers Build() clears),
        // so they're wired once here instead of re-subscribed per Build() to avoid stacking handlers.
        string _answerParamName;

        public override void _Ready()
        {
            _submitContainer.FindObjectsByType<Button>().First().Pressed += () => EmitSignal(SignalName.OnSubmit);
            _answerSpinbox.ValueChanged += value =>
            {
                if (_answerParamName == null)
                    return;
                EmitSignal(SignalName.OnValueChanged, _answerParamName, ((int)value).ToString());
            };
        }

        public class Builder
        {
            public enum Location
            {
                CHALLENGE,
                ANSWER,
                QUESTION
            }
            private Control _challengeContainer;
            private Control _submitContainer;
            private Control _questionContainer;
            Control _answerContainer;
            private readonly ChallengeUI _ui;
            private readonly IMathChallenge _challenge;

            public Builder(IMathChallenge challenge)
            {
                _challenge = challenge;

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                _ui = tree.FindAnyObjectByType<ChallengeUI>();

                _challengeContainer = _ui._challengeContainer;
                _submitContainer = _ui._submitContainer;
                _questionContainer = _ui._questionContainer;
                _answerContainer = _ui._answerContainer;

                _ui._answerSpinbox.Visible = false;
                _ui._answerParamName = null;
            }
            public ChallengeUI Build()
            {
                var ui = _ui;

                ui
                    .FindObjectsByType<ChallengeValueSelect>()
                    .ToList()
                    .ForEach(select => select.OnValueSelected += (index) => ui.EmitSignal(SignalName.OnValueSelected, index));

                ui
                    .FindObjectsByType<ChallengeParamInput>()
                    .ToList()
                    .ForEach(input => input.OnParamChanged += (param, value) => ui.EmitSignal(SignalName.OnValueChanged, param, value));

                return ui;
            }

            private Control GetContainer(Location location)
            {
                switch (location)
                {
                    case Location.ANSWER: return _answerContainer;
                    case Location.QUESTION: return _questionContainer;
                    case Location.CHALLENGE:
                    default: return _challengeContainer;
                }
            }

            public Builder WithValueSelect(Func<IMathChallenge, ChallengeValueSelect> createValueSelect, Location location = Location.CHALLENGE)
            {
                var container = GetContainer(location);

                container.Clear();
                container.AddChild(createValueSelect.Invoke(_challenge));

                return this;
            }
            public Builder WithValueView(Func<IMathChallenge, ChallengeValueView> createValueView, Location location = Location.CHALLENGE)
            {
                var container = GetContainer(location);

                container.Clear();
                container.AddChild(createValueView.Invoke(_challenge));

                return this;
            }
            public Builder WithParamInputs(Func<IMathChallenge, ChallengeParamInput> createParamInput, Location location = Location.CHALLENGE)
            {
                var container = GetContainer(location);

                container.Clear();
                container.AddChild(createParamInput.Invoke(_challenge));

                return this;
            }
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
            public Builder WithSubmitButton()
            {
                _submitContainer.FindObjectsByType<Button>().First().Disabled = _challenge.Values.Count(v => v.ParamName != "") != _challenge.FormulaParams.Length;

                return this;
            }
            public Builder WithSpinboxAnswer()
            {
                _ui._answerSpinbox.Visible = true;
                _ui._answerSpinbox.SetValueNoSignal(0);
                _ui._answerParamName = _challenge.FormulaParams[0];

                return this;
            }
        }
    }
}