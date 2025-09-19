

using System;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass, Tool]
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
        Dictionary<ChallengeUIType, Resource> _challengeScenes;

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
            private readonly IMathChallenge _challenge;

            public Builder(IMathChallenge challenge)
            {
                _challenge = challenge;

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                var ui = tree.FindAnyObjectByType<ChallengeUI>();

                _challengeContainer = ui._challengeContainer;
                _submitContainer = ui._submitContainer;
                _questionContainer = ui._questionContainer;
                _answerContainer = ui._answerContainer;
            }
            public ChallengeUI Build()
            {

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                var ui = tree.FindAnyObjectByType<ChallengeUI>();

                _submitContainer
                    .FindObjectsByType<Button>()
                    .ToList()
                    .ForEach(button => button.Pressed += () => ui.EmitSignal(SignalName.OnSubmit));

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
                _submitContainer.Clear();
                var btn = _submitContainer.CreateChild<Button>();
                btn.Text = "Klaar!";
                btn.Disabled = _challenge.Values.Where(v => v.ParamName != "").Count() != _challenge.FormulaParams.Length;

                return this;
            }
        }
    }
}