using System.Threading.Tasks;
using TnT.Systems.State;
using Godot;
using TnT.EduGame.Challenges;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
{
    [GlobalClass]
    public partial class GameStateChallenge : BaseGameState, IStateObject<GameStateChallenge.ChallengeOptions>
    {
        public struct ChallengeOptions
        {
            public MathChallenge challenge;
            public int seed;
        }

        public BaseState GetState(ChallengeOptions options)
        {
            ChallengeController.Instance.SetChallenge(options.challenge, options.seed);
            return new BaseState(new() { OnEnter = StartChallenge, OnResume = ResumeChallenge, OnExit = CloseChallenge });
        }

        async Task StartChallenge()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            await ChallengeController.Instance.Show();

            await Task.Yield();
        }

        // a state on top of it closed and may have unpaused the tree
        Task ResumeChallenge()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            return Task.CompletedTask;
        }

        async Task CloseChallenge()
        {
            await ChallengeController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
        }
    }
}