using System.Threading.Tasks;
using Godot;
using TnT.EduGame.GameState;
using TnT.Systems.Persistence;
using TnT.Systems.UI;

namespace TnT.EduGame.Question
{
    [GlobalClass]
    public partial class MathChallengeTrigger : Node, IBind<ChallengeSaveData>
    {
        [Export] MathChallenge _challenge;

        [Signal] public delegate void ChallengeCompletedEventHandler();
        [Signal] public delegate void ChallengeFailedEventHandler();

        public string PersistentId { get; private set; }

        // replaced by the persisted data once SaveLoadManager binds this trigger
        ChallengeSaveData _saveData = new();

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

        // Shows the challenge; true when solved, false when failed
        public async Task<bool> RunChallengeAsync()
        {
            var answered = new TaskCompletionSource<bool>();
            void OnCompleted() => answered.TrySetResult(true);
            void OnFailed() => answered.TrySetResult(false);

            // subscribe before triggering so a same-frame answer isn't missed
            ChallengeCompleted += OnCompleted;
            ChallengeFailed += OnFailed;
            try
            {
                TriggerChallenge();
                return await answered.Task;
            }
            finally
            {
                ChallengeCompleted -= OnCompleted;
                ChallengeFailed -= OnFailed;
            }
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
