using System.Threading.Tasks;
using Godot;
using TnT.EduGame.GameStates;
using TnT.Systems.Persistence;
using TnT.EduGame.UI;
namespace TnT.EduGame.Challenges
{
    [GlobalClass]
    public partial class MathChallengeTrigger : Node, IBind<ChallengeSaveData>
    {
        [Export] MathChallenge _challenge;

        public MathChallenge Challenge
        {
            get => _challenge;
            set => _challenge = value;
        }

        [Signal] public delegate void ChallengeCompletedEventHandler();
        [Signal] public delegate void ChallengeFailedEventHandler();

        public string PersistentId { get; private set; }

        // replaced by the persisted data once SaveLoadManager binds this trigger
        ChallengeSaveData _saveData = new();

        public override void _EnterTree()
        {
            PersistentId = GetPath().ToString();
        }

        // A new round seed: picked, set from outside, or a different one from the save data.
        // Clues showing this trigger's values listen to it to redraw.
        [Signal] public delegate void SeedChangedEventHandler();

        // The seed of the round this trigger shows. Kept until the round is solved, so a failed
        // attempt replays the same values. Reading it picks one when none is pending, so a clue can
        // show the values before the challenge has ever run; setting it (e.g. the dungeon populator,
        // so a door and its clue share a seed) only holds until a saved pending seed is bound.
        public int Seed
        {
            get
            {
                if (!_saveData.HasPendingSeed)
                    SetSeed((int)GD.Randi());
                return _saveData.PendingSeed;
            }
            set => SetSeed(value);
        }

        // The round the challenge will show (same values as the UI). Null without a challenge.
        public ChallengeRound Round => _challenge?.Roll(Seed);

        // Bound on SceneLoaded, which can be after a clue read the seed or the populator set it:
        // a saved pending seed wins, otherwise the seed picked or set so far carries over.
        public void Bind(ChallengeSaveData data)
        {
            var early = _saveData;
            _saveData = data;
            if (data.HasPendingSeed)
            {
                if (!early.HasPendingSeed || early.PendingSeed != data.PendingSeed)
                    EmitSignal(SignalName.SeedChanged);
            }
            else if (early.HasPendingSeed)
            {
                data.PendingSeed = early.PendingSeed;
                data.HasPendingSeed = true;
            }
        }

        void SetSeed(int seed)
        {
            bool changed = !_saveData.HasPendingSeed || _saveData.PendingSeed != seed;
            _saveData.PendingSeed = seed;
            _saveData.HasPendingSeed = true;
            if (changed)
                EmitSignal(SignalName.SeedChanged);
        }

        public void TriggerChallenge()
        {
            ChallengeController.Instance.Answered += OnAnswered;
            StateManagerGame.Instance.ShowChallenge((MathChallenge)_challenge.Duplicate(), Seed);
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
