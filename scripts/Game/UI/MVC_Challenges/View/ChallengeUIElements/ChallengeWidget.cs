using System;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Shared base of everything ChallengeUI puts in a slot: gets the challenge, the
    // rolled round and the element (visualization / input resource) it renders.
    public partial class ChallengeWidget : Control
    {
        // Set by ChallengeUI right after Instantiate(), before the widget enters
        // the tree, so they're available to subclasses by the time their _Ready() runs.
        protected MathChallenge Challenge { get; private set; }
        protected ChallengeRound Round { get; private set; }
        protected ChallengeElement Element { get; private set; }

        public void Init(MathChallenge challenge, ChallengeRound round, ChallengeElement element)
        {
            if (!Accepts(element))
                throw new InvalidOperationException(
                    $"{GetType().Name} can't render a {element.GetType().Name}; check the element's Scene override");

            Challenge = challenge;
            Round = round;
            Element = element;
        }

        protected virtual bool Accepts(ChallengeElement element) => true;
    }
}
