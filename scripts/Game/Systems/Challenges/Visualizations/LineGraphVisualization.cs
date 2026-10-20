using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace TnT.EduGame.Question
{
    // Line graph of an expression in a variable over a labelled grid, e.g. oil level
    // `v - r * t` over days `t`, or RPM `s * t` over wind speed. Optional marker at MarkX
    // for "what is the value at ...?" questions. The line may leave the graph (it is
    // clipped); only the marked point has to be on the graph and on a gridline.
    [GlobalClass, Tool]
    public partial class LineGraphVisualization : ChallengeVisualization
    {
        // y as an expression of Variable and the formula's params
        [Export] public string Line { get; set; } = "a + b * t";
        [Export] public string Variable { get; set; } = "t";

        [Export] public int XMin { get; set; }
        [Export] public int XMax { get; set; } = 10;
        [Export] public int XStep { get; set; } = 1;
        [Export] public int YMin { get; set; }
        [Export] public int YMax { get; set; } = 100;
        [Export] public int YStep { get; set; } = 10;

        [Export] public string XTitle { get; set; } = "";
        [Export] public string YTitle { get; set; } = "";

        // x of the marked point: a number or an expression of params (e.g. `6` or `d`); empty for no marker
        [Export] public string MarkX { get; set; } = "";

        // `{param}` placeholders allowed; shown at the marker
        [Export] public string MarkLabel { get; set; } = "?";

        // the marked point lands on a gridline, so it can be read off
        [Export] public bool SnapToGrid { get; set; } = true;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/LineGraph.tscn";

        public override IEnumerable<string> ImpliedConstraints()
        {
            if (MarkX == "")
                yield break;

            var x = $"({MarkX})";
            var y = $"({LineAt(x)})";
            yield return $"{x} >= {XMin} AND {x} <= {XMax}";
            yield return $"{y} >= {YMin} AND {y} <= {YMax}";
            if (SnapToGrid && XStep > 0)
                yield return $"({x} - ({XMin})) % {XStep} = 0";
            if (SnapToGrid && YStep > 0)
                yield return $"({y} - ({YMin})) % {YStep} = 0";
        }

        // Line with Variable replaced by `x` (a number or expression)
        public string LineAt(string x) => Regex.Replace(Line, $@"\b{Regex.Escape(Variable)}\b", $"({x})");

        public bool TryValueAt(ChallengeRound round, double x, out double y) =>
            FormulaEvaluator.TryEvaluateNumber(LineAt(x.ToString(System.Globalization.CultureInfo.InvariantCulture)), round.Values, out y);

        public bool TryMarkedX(ChallengeRound round, out int x)
        {
            x = 0;
            return MarkX != "" && FormulaEvaluator.TryEvaluate(MarkX, round.Values, out x);
        }
    }
}
