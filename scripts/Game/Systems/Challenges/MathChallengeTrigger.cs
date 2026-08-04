using System;
using Godot;
using TnT.EduGame.GameState;
using TnT.EduGame.QuestSystem;
using TnT.Systems.UI;

namespace TnT.EduGame.Question
{
    [GlobalClass]
    public partial class MathChallengeTrigger : Node, IQuestReactionObject
    {
        [Export] MathChallenge _challenge;

        [Signal] public delegate void ChallengeCompletedEventHandler();

        public event Action ReactionCompleted
        {
            add => Connect(SignalName.ChallengeCompleted, Callable.From(value));
            remove => Disconnect(SignalName.ChallengeCompleted, Callable.From(value));
        }

        public void TriggerChallenge()
        {
            GD.Print($"TriggerChallenge: {_challenge.Name}");
            GD.Print($"TriggerChallenge: {ChallengeController.Instance}");
            GD.Print($"TriggerChallenge: {StateManagerGame.Instance}");
            GD.Print($"TriggerChallenge: {_challenge}");
            ChallengeController.Instance.Answered += OnAnswered;
            StateManagerGame.Instance.ShowChallenge((MathChallenge)_challenge.Duplicate());
        }

        private async void OnAnswered(bool isCorrect)
        {
            if (!isCorrect) return;

            ChallengeController.Instance.Answered -= OnAnswered;
            await StateManagerGame.Instance.Pop();
            EmitSignal(SignalName.ChallengeCompleted);
        }
    }
}
