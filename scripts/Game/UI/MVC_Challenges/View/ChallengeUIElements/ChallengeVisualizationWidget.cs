using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Base for anything placed in ChallengeUI's visualization slot: renders the
    // round's givens and hides its unknowns. A widget that also lets the player
    // answer implements IChallengeInput, and the builder then skips the input slot.
    public partial class ChallengeVisualizationWidget : Control
    {
        // Set by the builder right after Instantiate(), before the widget enters
        // the tree, so they're available to subclasses by the time their _Ready() runs.
        protected IMathChallenge Challenge { get; private set; }
        protected ChallengeRound Round { get; private set; }

        public void Init(IMathChallenge challenge, ChallengeRound round)
        {
            Challenge = challenge;
            Round = round;
        }
    }
}
