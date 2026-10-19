

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

        // Fills the three slots for this round; a visualization that doubles as
        // input (IChallengeInput) takes the place of the input slot.
        public void ShowRound(MathChallenge challenge, ChallengeRound round)
        {
            ShowQuestion(challenge.QuestionFor(round));

            var visualization = AddWidget<ChallengeVisualizationWidget>(_visualizationContainer, challenge.VisualizationOrNone, challenge, round);

            _inputContainer.Clear();
            if (visualization is IChallengeInput ownInput)
            {
                if (challenge.Input != null)
                    GD.PushWarning($"{challenge.Name}: visualization already handles input, ignoring {challenge.Input.GetType().Name}");
                Wire(ownInput);
                return;
            }

            if (challenge.Input == null)
                throw new InvalidOperationException($"{challenge.Name}: no Input assigned and the visualization doesn't handle input");
            Wire(AddWidget<ChallengeInputWidget>(_inputContainer, challenge.Input, challenge, round));
        }

        void ShowQuestion(string text)
        {
            _questionContainer.Clear();

            var label = _questionContainer.CreateChild<RichTextLabel>();
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            label.SizeFlagsVertical = SizeFlags.ExpandFill;
            label.SetAnchorsPreset(LayoutPreset.FullRect);
            label.Text = text;
        }

        // replaces the slot's content with the element's widget scene
        static T AddWidget<T>(Control slot, ChallengeElement element, MathChallenge challenge, ChallengeRound round) where T : ChallengeWidget
        {
            slot.Clear();

            var widget = element.ResolveScene()?.Instantiate() as T
                ?? throw new InvalidOperationException($"{element.GetType().Name}: scene is missing or its root is not a {typeof(T).Name}");
            widget.Init(challenge, round, element);
            slot.AddChild(widget);
            return widget;
        }

        void Wire(IChallengeInput input)
        {
            input.ValueAssigned += (paramName, value) => EmitSignal(SignalName.OnValueAssigned, paramName, value);
            input.ValueCleared += paramName => EmitSignal(SignalName.OnValueCleared, paramName);
        }
    }
}
