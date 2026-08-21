using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    public partial class ChallengeController : Node
    {
        public static ChallengeController Instance { get; private set; }
        [Export]
        public ChallengeView view = new();
        [Export]
        public ChallengeModel model = new();

        [Signal] public delegate void AnsweredEventHandler(bool isCorrect);

        public override void _Ready()
        {
            Instance = this;
            Initialize();
        }

        public void SetChallenge(IMathChallenge challenge)
        {
            model.SetChallenge(challenge as MathChallenge);
            Refresh();
        }

        async void Initialize()
        {
            await view.InitializeView(model.Challenge);

            // ChallengeUI is scene-singleton furniture, never recreated between
            // challenges, so it's wired once here rather than re-subscribed on
            // every Refresh (which used to stack handlers per challenge shown).
            view.ChallengeUI.OnValueAssigned += ValueAssigned;
            view.ChallengeUI.OnSubmit += SubmitChallenge;
        }

        private void ValueAssigned(string paramName, int value)
        {
            model.AssignValue(paramName, value);
            view.ChallengeUI.SetSubmitEnabled(model.IsComplete);
        }

        private void SubmitChallenge()
        {
            try
            {
                var isCorrect = model.Challenge.CheckAnswer();
                GD.Print($"Is Correct: {isCorrect}");
                EmitSignal(SignalName.Answered, isCorrect);
            }
            catch(Exception ex)
            {
                GD.PrintErr($"Error: {ex}");
            }
        }

        void Refresh()
        {
            view.Refresh(model.Challenge);
            view.ChallengeUI.SetSubmitEnabled(model.IsComplete);
        }

        public async Task Show()
        {
            await Task.Yield();
            await view.ShowView();
        }

        public async Task Hide()
        {
            await view.HideView();
            await Task.Yield();
        }
    }
}
