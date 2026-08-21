using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.EduGame.Question;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStateChallenge : BaseGameState, IStateObject<GameStateChallenge.ChallengeOptions>
    {
        public struct ChallengeOptions
        {
            public IMathChallenge challenge;
        }

        public BaseState GetState(ChallengeOptions options)
        {
            ChallengeController.Instance.SetChallenge(options.challenge);
            return new BaseState(new() { OnEnter = StartChallenge, OnExit = CloseChallenge });
        }

        async Task StartChallenge()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            await ChallengeController.Instance.Show();

            await Task.Yield();
        }

        async Task CloseChallenge()
        {
            await ChallengeController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
        }
    }
}