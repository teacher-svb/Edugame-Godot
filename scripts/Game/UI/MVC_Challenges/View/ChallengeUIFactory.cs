
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    public static class ChallengeUIFactory
    {
        public static ChallengeUI Build(IMathChallenge challenge)
        {
            if (!ChallengeUIRegistry.TryGetStrategy(challenge.ChallengeUIType, out var strategy))
                throw new NotSupportedException($"No visualization strategy registered for {challenge.ChallengeUIType}");

            return strategy.Build(challenge);
        }
    }
}