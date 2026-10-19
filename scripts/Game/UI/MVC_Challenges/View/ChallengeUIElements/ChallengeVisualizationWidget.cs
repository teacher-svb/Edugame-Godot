using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Base for anything placed in ChallengeUI's visualization slot: renders the
    // round's givens and hides its unknowns. A widget that also lets the player
    // answer implements IChallengeInput, and ChallengeUI then skips the input slot.
    // Used as-is (empty) by NoVisualization.
    public partial class ChallengeVisualizationWidget : ChallengeWidget
    {
    }
}
