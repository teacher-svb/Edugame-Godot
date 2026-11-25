using Godot;

namespace TnT.EduGame.Challenges
{
    // A key whose teeth are the bars of a bar graph: each GraphBar is one tooth, its param the
    // tooth's depth on 0..AxisMax. The widget doubles as input: the player sets the depth of
    // every unknown tooth. Rolling and the implied rules come from BarGraphVisualization.
    // E.g. bars `a b c d`, formula `a*1000 + b*100 + c*10 + d`, unknowns `a b c d`: one key per
    // number, so cutting the digits of the clue's number is the only key that fits.
    [GlobalClass, Tool]
    public partial class KeyVisualization : BarGraphVisualization
    {
        // depth 0..9 in steps of 1 on the grid, no values written (see BarGraphVisualization's defaults)
        protected override int DefaultAxisMax => 9;
        protected override int DefaultAxisStep => 1;
        protected override bool DefaultSnapToGrid => true;
        protected override bool DefaultShowValues => false;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Key.tscn";
    }
}
