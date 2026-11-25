

using System;
using System.Linq;
using Godot;
using TnT.EduGame.Challenges;
using TnT.Extensions;

namespace TnT.EduGame.UI
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
        /// <summary>The hint toggle (the avocado, top right, shortcut Q): pressed shows the round's hint.</summary>
        [Export]
        BaseButton _hintToggle;

        Button _submitButton;
        // the widget the player answers with this round (input slot, or a visualization that doubles as input)
        ChallengeWidget _inputWidget;
        Control _hintPanel;
        RichTextLabel _hintLabel;

        public override void _Ready()
        {
            _submitButton = _submitContainer.FindObjectsByType<Button>(includeSelf: false).First();
            _submitButton.Pressed += () => EmitSignal(SignalName.OnSubmit);

            if (_hintToggle != null)
            {
                _hintToggle.ToggleMode = true;
                _hintToggle.Toggled += pressed => _hintPanel.Visible = pressed && !string.IsNullOrEmpty(_hintLabel.Text);
                BuildHintPanel();
                SetHint(null);   // no button until a round with a hint
            }
        }

        public void SetSubmitEnabled(bool enabled) => _submitButton.Disabled = !enabled;

        // Enter submits wherever the focus is (typing in a text input, a dropdown or cog with focus):
        // _Input runs before the focused control would take Enter for itself. An open dropdown list
        // is its own window and gets the key first. Same key as the button's shortcut, which this replaces.
        public override void _Input(InputEvent @event)
        {
            if (@event is not InputEventKey { Pressed: true, Echo: false } || !IsVisibleInTree() || _submitButton.Disabled
                || _submitButton.Shortcut?.MatchesEvent(@event) != true)
                return;
            GetViewport().SetInputAsHandled();
            EmitSignal(SignalName.OnSubmit);
        }

        /// <summary>Keyboard focus to the round's input (its first field, wheel, button or the key).</summary>
        public void FocusInput()
        {
            var target = _inputWidget != null && IsInstanceValid(_inputWidget) ? _inputWidget.FocusTarget : null;
            if (target != null && IsInstanceValid(target) && target.IsVisibleInTree())
                target.GrabFocus();
        }

        public void ReleaseInputFocus()
        {
            var owner = GetViewport().GuiGetFocusOwner();
            if (owner != null && IsAncestorOf(owner))
                owner.ReleaseFocus();
        }

        /// <summary>
        /// The round's hint behind the toggle (closed at the start of each round); empty: no toggle at all.
        /// </summary>
        public void SetHint(string hint)
        {
            if (_hintToggle == null)
                return;
            bool hasHint = !string.IsNullOrWhiteSpace(hint);
            _hintLabel.Text = hasHint ? hint : "";
            _hintToggle.SetPressedNoSignal(false);
            _hintToggle.Visible = hasHint;
            _hintToggle.Disabled = !hasHint;   // its Q shortcut does nothing either
            _hintPanel.Visible = false;
        }

        // a speech-bubble-like panel under the toggle, sized to the text
        void BuildHintPanel()
        {
            _hintPanel = new PanelContainer
            {
                Name = "HintPanel",
                ThemeTypeVariation = "LightPanel",
                Visible = false,
                MouseFilter = MouseFilterEnum.Ignore,
                ZIndex = 10,
            };
            AddChild(_hintPanel);
            _hintPanel.SetAnchorsPreset(LayoutPreset.TopRight);
            _hintPanel.GrowHorizontal = GrowDirection.Begin;
            _hintPanel.OffsetLeft = -420;
            _hintPanel.OffsetRight = -16;
            _hintPanel.OffsetTop = _hintToggle.OffsetBottom + 8;

            var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
            foreach (var side in new[] { "margin_left", "margin_top", "margin_right", "margin_bottom" })
                margin.AddThemeConstantOverride(side, 16);
            _hintPanel.AddChild(margin);

            _hintLabel = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                MouseFilter = MouseFilterEnum.Ignore,
            };
            margin.AddChild(_hintLabel);
        }

        // Fills the three slots for this round; a visualization that doubles as
        // input (IChallengeInput) takes the place of the input slot.
        public void ShowRound(MathChallenge challenge, ChallengeRound round)
        {
            _inputWidget = null;
            ShowQuestion(challenge.QuestionFor(round));

            var visualization = AddWidget<ChallengeVisualizationWidget>(_visualizationContainer, challenge.VisualizationOrNone, challenge, round);

            _inputContainer.Clear();
            var input = challenge.Input;
            if (visualization is IChallengeInput ownInput)
            {
                // e.g. a key whose Unknowns name a param that isn't a tooth: it could never be
                // submitted, and the challenge can't be closed, so the game would be stuck
                var uncovered = round.Unknowns.Where(u => !ownInput.Handles(u)).ToList();
                if (uncovered.Count == 0)
                {
                    if (input != null)
                        GD.PushWarning($"{challenge.Name}: visualization already handles input, ignoring {input.GetType().Name}");
                    Wire(ownInput);
                    _inputWidget = visualization;
                    FocusInputIfShown();
                    return;
                }

                input ??= new TextChallengeInput();
                GD.PushError($"{challenge.Name}: {challenge.VisualizationOrNone.GetType().Name} can't take {string.Join(", ", uncovered)} "
                    + $"(check Unknowns); answering with {input.GetType().Name} instead");
            }

            if (input == null)
                throw new InvalidOperationException($"{challenge.Name}: no Input assigned and the visualization doesn't handle input");
            var inputWidget = AddWidget<ChallengeInputWidget>(_inputContainer, input, challenge, round);
            Wire(inputWidget);
            _inputWidget = inputWidget;
            FocusInputIfShown();
        }

        // a round shown while the view is already open (normally rounds are set while it's hidden,
        // and ChallengeView.ShowView focuses); deferred so the widget's _Ready has built it
        void FocusInputIfShown()
        {
            if (IsVisibleInTree())
                Callable.From(FocusInput).CallDeferred();
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

            var node = element.ResolveScene()?.Instantiate();
            if (node is not T widget)
            {
                node?.Free();
                throw new InvalidOperationException($"{element.GetType().Name}: scene is missing or its root is not a {typeof(T).Name}");
            }

            try
            {
                widget.Init(challenge, round, element);
            }
            catch
            {
                widget.Free();   // never entered the tree, nothing else frees it
                throw;
            }
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
