
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    public enum ChallengeUIType
    {
        CogwheelUIStrategy,
        RadarUIStrategy,
        SearchGridUIStrategy,
        DropdownUIStrategy,
        TextInputUIStrategy,
        CombinationLockUIStrategy
    }

    public interface IChallengeUIStrategy
    {
        ChallengeUI Build(IMathChallenge challenge);
    }

    // Every strategy now does the same thing: put up the question, then instance
    // whichever widget scene ChallengeUI._challengeScenes has assigned to this
    // enum value (see Scenes/UI/ChallengeStrategies/*.tscn). The class-per-type
    // split still exists so ChallengeUIRegistry can keep validating a 1:1 mapping
    // between ChallengeUIType and a registered visualization at startup.

    public class CogwheelUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }

    public class RadarUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }

    public class SearchGridUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }

    public class DropdownUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }

    public class TextInputUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }

    public class CombinationLockUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge) =>
            new ChallengeUI.Builder(challenge)
                .WithQuestionElement()
                .WithSceneWidget()
                .Build();
    }
}
