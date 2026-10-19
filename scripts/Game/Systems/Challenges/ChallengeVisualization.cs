using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Question
{
    // How a challenge is shown. Concrete subclasses carry their own settings
    // (grid size, image, ring count, ...) and point at the scene that renders
    // them. A visualization also decides the round's values: the default rolls
    // every param from its ParamSpec, subclasses override Generate to add their
    // own constraints (e.g. SearchGrid clamping to the grid bounds).
    [GlobalClass, Tool]
    public abstract partial class ChallengeVisualization : ChallengeElement, IParamFieldOwner
    {
        public virtual ChallengeRound Generate(MathChallenge challenge, int seed) =>
            ChallengeRound.Roll(challenge.Formula, challenge.Params, challenge.Unknowns, seed, ConstraintsFor(challenge));

        // rules this visualization needs on top of the challenge's own, e.g. the radar's blip within range
        public virtual IEnumerable<IChallengeConstraint> ImpliedConstraints() => [];

        protected IEnumerable<IChallengeConstraint> ConstraintsFor(MathChallenge challenge) =>
            challenge.Constraints.Where(c => c != null).Concat(ImpliedConstraints());

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
