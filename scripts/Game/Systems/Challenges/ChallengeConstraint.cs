using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Authorable rule on a MathChallenge's rounds; see IChallengeConstraint.
    [GlobalClass, Tool]
    public abstract partial class ChallengeConstraint : Resource, IChallengeConstraint, IParamFieldOwner
    {
        public abstract bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values);
        public abstract IEnumerable<string> Params { get; }
        public abstract string Describe();

        // filled by the owning MathChallenge, so the fields in ParamFields get a dropdown
        string[] _availableParams = [];

        public void SetAvailableParams(string[] names)
        {
            _availableParams = names;
            NotifyPropertyListChanged();
        }

        // names of this resource's string properties that hold a param name
        protected virtual IEnumerable<string> ParamFields => [];

        public override void _ValidateProperty(Godot.Collections.Dictionary property) =>
            ParamFieldHints.Apply(property, ParamFields, _availableParams);
    }
}
