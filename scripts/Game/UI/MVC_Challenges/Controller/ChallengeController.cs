using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Challenges;

namespace TnT.EduGame.UI
{
    public partial class ChallengeController : Node
    {
        public static ChallengeController Instance { get; private set; }
        // assigned in managers.tscn; no `= new()` defaults, those created nodes nobody freed
        [Export]
        public ChallengeView view;
        [Export]
        public ChallengeModel model;

        [Signal] public delegate void AnsweredEventHandler(bool isCorrect);

        // from Show until the answer or Hide
        bool _open;

        public override void _Ready()
        {
            Instance = this;
            Initialize();
        }

        public void SetChallenge(MathChallenge challenge, int seed)
        {
            model.SetChallenge(challenge, seed);
            Refresh();
        }

        // synchronous, so a challenge shown in the first frame (a dev checkpoint, a trigger in a
        // level's _Ready) finds the view wired
        void Initialize()
        {
            view.InitializeView();

            // ChallengeUI is scene-singleton furniture, never recreated between
            // challenges, so it's wired once here rather than re-subscribed on
            // every Refresh (which used to stack handlers per challenge shown).
            view.ChallengeUI.OnValueAssigned += ValueAssigned;
            view.ChallengeUI.OnValueCleared += ValueCleared;
            view.ChallengeUI.OnSubmit += SubmitChallenge;
            view.ChallengeUI.SetSubmitEnabled(false);   // nothing to submit before the first round
        }

        private void ValueAssigned(string paramName, int value)
        {
            model.AssignValue(paramName, value);
            UpdateSubmit();
        }

        private void ValueCleared(string paramName)
        {
            model.ClearValue(paramName);
            UpdateSubmit();
        }

        private void SubmitChallenge()
        {
            if (!_open)
                return;   // hiding or hidden: the answer is in already
            if (model.Round == null)
            {
                GD.PushWarning("ChallengeController: submit without a challenge round, ignored");
                return;
            }

            bool isCorrect;
            try
            {
                isCorrect = model.CheckAnswer();
            }
            catch (Exception ex)
            {
                // a broken challenge must not trap the player in the overlay:
                // report it loudly and let the game carry on as a wrong answer
                GD.PushError($"Checking challenge '{model.ChallengeName}' failed: {ex}");
                isCorrect = false;
            }
            // one answer per showing: the challenge closes now (possibly only after the rest of its
            // open animation), and nothing may answer again before the next Show
            _open = false;
            UpdateSubmit();
            EmitSignal(SignalName.Answered, isCorrect);
        }

        void Refresh()
        {
            view.Refresh(model.Challenge, model.Round);
            UpdateSubmit();
            view.ChallengeUI.SetHint(model.Hint);
        }

        // Submit only while the challenge is shown and every unknown has a value. A value reported
        // while it scales out (e.g. the text field's resync when HideView releases its focus) must not
        // enable it again, or a second Enter would answer twice.
        void UpdateSubmit() => view.ChallengeUI.SetSubmitEnabled(_open && model.IsComplete);

        public Task Show()
        {
            _open = true;
            UpdateSubmit();   // start values reported before the view opened (lock, key)
            return view.ShowView();
        }

        public Task Hide()
        {
            _open = false;
            return view.HideView();
        }
    }
}
