using System;
using Godot;
using TnT.EduGame.GameState;
using TnT.EduGame.QuestSystem;
using TnT.Systems.Persistence;
using TnT.Systems.UI;

namespace TnT.EduGame.Question
{
    [GlobalClass]
    public partial class MathChallengeTrigger : Node, IQuestReactionObject, IBind<ChallengeSaveData>
    {
        [Export] MathChallenge _challenge;

        [Signal] public delegate void ChallengeCompletedEventHandler();
        [Signal] public delegate void ChallengeFailedEventHandler();

        public string PersistentId { get; private set; }

        // replaced by the persisted data once SaveLoadManager binds this trigger
        ChallengeSaveData _saveData = new();

        public event Action ReactionCompleted
        {
            add => Connect(SignalName.ChallengeCompleted, Callable.From(value));
            remove => Disconnect(SignalName.ChallengeCompleted, Callable.From(value));
        }

        public override void _EnterTree()
        {
            PersistentId = GetPath().ToString();
        }

        public void Bind(ChallengeSaveData data) => _saveData = data;

        public void TriggerChallenge()
        {
            // keep the seed until the round is solved, so a failed attempt replays the same values
            if (!_saveData.HasPendingSeed)
            {
                _saveData.PendingSeed = (int)GD.Randi();
                _saveData.HasPendingSeed = true;
            }

            ChallengeController.Instance.Answered += OnAnswered;
            StateManagerGame.Instance.ShowChallenge((MathChallenge)_challenge.Duplicate(), _saveData.PendingSeed);
        }

        private async void OnAnswered(bool isCorrect)
        {
            ChallengeController.Instance.Answered -= OnAnswered;
            if (isCorrect)
                _saveData.HasPendingSeed = false;

            await StateManagerGame.Instance.Pop();
            EmitSignal(isCorrect ? SignalName.ChallengeCompleted : SignalName.ChallengeFailed);
        }
    }
}
