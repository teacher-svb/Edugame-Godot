using System;
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

        // A one-shot challenge (CodeDoor sets it): a correct answer keeps the seed, so its clue keeps
        // showing the solved values, also after save/load. Off (reusable quest challenges): a correct
        // answer drops the seed and the next run, and its clues, get new values.
        [Export] public bool KeepSolvedSeed { get; set; }

        [Signal] public delegate void ChallengeCompletedEventHandler();
        [Signal] public delegate void ChallengeFailedEventHandler();

        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time

        // replaced by the persisted data once SaveLoadManager binds this trigger
        ChallengeSaveData _saveData = new();

        // A new round seed: picked, set from outside, or a different one from the save data.
        // Clues showing this trigger's values listen to it to redraw.
        [Signal] public delegate void SeedChangedEventHandler();

        // The seed of the round this trigger shows. Kept until the round is solved, so a failed
        // attempt replays the same values. Reading it picks one when none is pending, so a clue can
        // show the values before the challenge has ever run (without SeedChanged: whoever reads it
        // uses it right away, and a clue's Refresh reads it); setting it (e.g. the dungeon populator,
        // so a door and its clue share a seed) only holds until a saved pending seed is bound.
        public int Seed
        {
            get
            {
                if (!_saveData.HasPendingSeed)
                {
                    _saveData.PendingSeed = (int)GD.Randi();
                    _saveData.HasPendingSeed = true;
                }
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

        // the next read picks a new seed; clues showing the old values redraw
        void DropSeed()
        {
            _saveData.HasPendingSeed = false;
            EmitSignal(SignalName.SeedChanged);
        }

        void SetSeed(int seed)
        {
            bool changed = !_saveData.HasPendingSeed || _saveData.PendingSeed != seed;
            _saveData.PendingSeed = seed;
            _saveData.HasPendingSeed = true;
            if (changed)
                EmitSignal(SignalName.SeedChanged);
        }

        public void TriggerChallenge() => TryTriggerChallenge();

        // false when it isn't shown: another challenge is already open, or this one can't be rolled
        // or shown (error logged). Answered is only subscribed once it is shown, so a failure can't
        // leave a handler behind that would also answer for the next challenge.
        bool TryTriggerChallenge()
        {
            if (_challenge == null)
            {
                GD.PushError($"{Name}: MathChallengeTrigger has no challenge");
                return false;
            }

            var seed = Seed;
            try
            {
                if (!StateManagerGame.Instance.ShowChallenge((MathChallenge)_challenge.Duplicate(), seed))
                    return false;
            }
            catch (Exception e)
            {
                // e.g. no valid round for this seed in the roll attempts: don't keep (and save) a
                // seed that fails every retry; a broken challenge fails with the next one too
                GD.PushError($"{Name}: challenge '{_challenge.Name}' could not be shown (seed {seed}): {e.Message}");
                DropSeed();
                return false;
            }
            ChallengeController.Instance.Answered += OnAnswered;
            return true;
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
                if (!TryTriggerChallenge())
                    return false;
                return await answered.Task;
            }
            finally
            {
                // a freed trigger (scene change) can't be unsubscribed from, and has nothing left to call
                if (IsInstanceValid(this))
                {
                    ChallengeCompleted -= OnCompleted;
                    ChallengeFailed -= OnFailed;
                }
            }
        }

        private async void OnAnswered(bool isCorrect)
        {
            ChallengeController.Instance.Answered -= OnAnswered;
            try
            {
                if (isCorrect && !KeepSolvedSeed)
                    DropSeed();   // solved: the next run rolls new values, and clues show those

                await StateManagerGame.Instance.CloseChallenge();   // the challenge state, not whatever is on top
            }
            catch (Exception e)
            {
                var what = IsInstanceValid(this) ? $"{Name}: closing challenge '{_challenge?.Name}'" : "MathChallengeTrigger: closing a challenge";
                GD.PushError($"{what} failed: {e}");
            }
            finally
            {
                // always report, or a waiting door (busy flag) or ChallengeStep would hang; unless the trigger was
                // freed during the close (scene change): its door and quest step are gone with it
                if (IsInstanceValid(this))
                    EmitSignal(isCorrect ? SignalName.ChallengeCompleted : SignalName.ChallengeFailed);
            }
        }
    }
}
