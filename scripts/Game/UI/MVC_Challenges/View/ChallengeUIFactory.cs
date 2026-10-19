
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    public static class ChallengeUIFactory
    {
        public static ChallengeUI Build(IMathChallenge challenge, ChallengeRound round) =>
            new ChallengeUI.Builder(challenge, round)
                .WithQuestion()
                .WithVisualization()
                .WithInput()
                .Build();
    }
}
