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
        [Signal] public delegate void ChallengeFailedEventHandler();

        public event Action ReactionCompleted
        {
            add => Connect(SignalName.ChallengeCompleted, Callable.From(value));
            remove => Disconnect(SignalName.ChallengeCompleted, Callable.From(value));
        }

        public void TriggerChallenge()
        {
            ChallengeController.Instance.Answered += OnAnswered;
            StateManagerGame.Instance.ShowChallenge((MathChallenge)_challenge.Duplicate());
        }

        private async void OnAnswered(bool isCorrect)
        {
            ChallengeController.Instance.Answered -= OnAnswered;
            await StateManagerGame.Instance.Pop();
            EmitSignal(isCorrect ? SignalName.ChallengeCompleted : SignalName.ChallengeFailed);
        }
    }
}
