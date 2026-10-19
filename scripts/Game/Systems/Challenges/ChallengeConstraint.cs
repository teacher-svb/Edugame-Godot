using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Authorable rule on a MathChallenge's rounds; see IChallengeConstraint.
    [GlobalClass, Tool]
    public abstract partial class ChallengeConstraint : Resource, IChallengeConstraint
    {
        public abstract bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values);
        public abstract IEnumerable<string> Params { get; }
        public abstract string Describe();
    }
}
