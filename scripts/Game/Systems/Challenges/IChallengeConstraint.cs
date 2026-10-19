using System.Collections.Generic;

namespace TnT.EduGame.Question
{
    // A rule every rolled round must satisfy (Specification pattern): rounds are
    // rerolled until all rules hold, and input options only offer values that keep
    // them true. Plain C# so ChallengeRound stays testable; the authorable rules are
    // ChallengeConstraint resources, visualizations add their own in code.
    public interface IChallengeConstraint
    {
        // `values` holds every param plus `answer`
        bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values);

        // params this rule reads, checked against the formula before rolling
        IEnumerable<string> Params { get; }

        // human-readable, e.g. "answer ≥ 0"; used in error messages
        string Describe();
    }
}
