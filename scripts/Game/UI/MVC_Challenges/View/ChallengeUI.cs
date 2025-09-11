

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
        Control _submitContainer;

        [Export]
        Dictionary<ChallengeUIType, Control> _test;

        [Export]
        public string _challengeUIType { get; set; } = "";




        public class Builder
        {
            private Control _challengeContainer;
            private Control _submitContainer;
            private Control _questionContainer;
            private readonly IMathChallenge _challenge;

            public Builder(IMathChallenge challenge)
            {
                _challenge = challenge;
            }
            public ChallengeUI Build()
            {

                SceneTree tree = (SceneTree)Engine.GetMainLoop();
                var ui = tree.FindAnyObjectByType<ChallengeUI>();
                // var ui = new ChallengeUI();

                // _left.AddClass("left");
                // _right.AddClass("right");

                _challengeContainer = ui._challengeContainer;
                _submitContainer = ui._submitContainer;
                _questionContainer = ui._questionContainer;

                ui
                    .FindObjectsByType<Button>()
                    .ToList()
                    .ForEach(button => button.Pressed += () => ui.EmitSignal(SignalName.OnSubmit));

                ui
                    .FindObjectsByType<ChallengeSelect>()
                    .ToList()
                    .ForEach(select => select.OnValueSelected += (index) => ui.EmitSignal(SignalName.OnValueSelected, index));

                ui
                    .FindObjectsByType<ChallengeParamInput>()
                    .ToList()
                    .ForEach(input => input.OnParamChanged += (param, value) => ui.EmitSignal(SignalName.OnValueChanged, param, value));


                return ui;
            }
            Builder WithChallengeVisual()
            {
                if (_challengeContainer != null)
                    return this;

                _challengeContainer = new Control();
                // _challengeContainer.AddClass("challengeContainer");

                return this;
            }
            public Builder WithValueSelect(Func<IMathChallenge, ChallengeSelect> createValueSelect)
            {
                _challengeContainer = createValueSelect.Invoke(_challenge);

                return this;
            }
            public Builder WithValueView(Func<IMathChallenge, ChallengeValueView> createValueView)
            {
                _challengeContainer = createValueView.Invoke(_challenge);

                return this;
            }
            public Builder WithParamInputs(Func<IMathChallenge, ChallengeParamInput> createParamInput)
            {
                _challengeContainer = createParamInput.Invoke(_challenge);

                return this;
            }
            public Builder WithQuestionElement()
            {
                if (_questionContainer != null)
                    return this;
                _questionContainer = new Control();
                // _questionContainer.AddClass("questionContainer");

                var text = _questionContainer.CreateChild<Label>();
                text.Text = _challenge.Question;

                return this;
            }
            public Builder WithAnswerInput()
            {
                if (_submitContainer == null)
                {
                    _submitContainer = new Control();
                    // _submitContainer.AddClass("submitContainer");
                }


                ChallengeParamInput input = new();

                _challenge.FormulaParams
                    .Select(p => new ChallengeValueInput(p))
                    .ToList()
                    .ForEach(p =>
                    {
                        // p.RegisterValueChangedCallback(i => input.OnParamChanged?.Invoke(p.ParamName, int.Parse(i.newValue)));
                        p.TextChanged += i => input.OnParamChanged?.Invoke(p.ParamName, i);
                        input.AddChild(p);
                        p.AddTo(input);
                    });

                _submitContainer.AddChild(input);

                return this;
            }
            public Builder WithSubmitButton()
            {
                if (_submitContainer == null)
                {
                    _submitContainer = new Control();
                    // _submitContainer.AddClass("submitContainer");
                }

                // _submitContainer = new Control();
                // _submitContainer.AddClass("submitContainer");

                var btn = _submitContainer.CreateChild<Button>();
                btn.Text = "Klaar!";
                btn.Disabled = _challenge.Values.Where(v => v.ParamName != "").Count() != _challenge.FormulaParams.Length;

                return this;
            }
        }
    }
}