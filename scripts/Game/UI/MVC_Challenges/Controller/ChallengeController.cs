using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeController : Node
    {
        [Export]
        public ChallengeView view = new();
        [Export]
        public ChallengeModel model = new();
        void Start()
        {
            Initialize();
        }

        async void Initialize()
        {
            await view.InitializeView(model.Challenge);
        }

        private void ValueChanged(string paramName, string value)
        {
            model.SetParameter(paramName, int.Parse(value));
            Refresh();
        }

        private void SubmitChallenge()
        {
            try
            {
                var isCorrect = model.Challenge.CheckAnswer();
                GD.Print($"Is Correct: {isCorrect}");
            }
            catch(Exception ex)
            {
                GD.PrintErr($"Error: {ex}");
            }
        }

        private void ValueSelected(int index)
        {
            model.SetParameter(index);
            Refresh();
        }

        void Refresh()
        {
            view.Refresh(model.Challenge);
            view.ChallengeUI.OnValueSelected += ValueSelected;
            view.ChallengeUI.OnValueChanged += ValueChanged;
            view.ChallengeUI.OnSubmit += SubmitChallenge;
        }

        public async Task ShowView()
        {
            await Task.Yield();
            view.ShowView();
        }

        public async Task HideView()
        {
            view.HideView();
            await Task.Yield();
        }
    }
}