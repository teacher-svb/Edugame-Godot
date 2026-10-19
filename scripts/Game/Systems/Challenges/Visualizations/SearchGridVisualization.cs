using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Coordinate grid (optionally over a map picture) with labelled lines and a
    // marker at one of the round's values, for "what is the X location of A?"
    // questions (e.g. formula `x`, unknown `answer`). Y counts upwards, like latitude.
    [GlobalClass, Tool]
    public partial class SearchGridVisualization : ChallengeVisualization
    {
        [Export(PropertyHint.Range, "1,20")] public int Columns { get; set; } = 10;
        [Export(PropertyHint.Range, "1,20")] public int Rows { get; set; } = 10;

        // value at the first line and the difference between two lines, per axis
        [Export] public int XStart { get; set; }
        [Export] public int XStep { get; set; } = 1;
        [Export] public int YStart { get; set; }
        [Export] public int YStep { get; set; } = 1;

        // optional picture (e.g. a map) stretched under the grid
        [Export] public Texture2D Background { get; set; }

        // value that gives the marker's X / Y; empty picks a grid line from the round's seed
        [Export] public string XParam { get; set; } = ChallengeRound.AnswerParam;
        [Export] public string YParam { get; set; } = "";

        // only accept rounds that put the marker exactly on line crossings, so the answer can be read off
        [Export] public bool SnapToLines { get; set; } = true;

        // `{param}` placeholders allowed; shown next to the marker, empty for none
        [Export] public string MarkerLabel { get; set; } = "";

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/SearchGrid.tscn";

        protected override IEnumerable<string> ParamFields => [nameof(XParam), nameof(YParam)];

        // the marker has to land on the grid (on line crossings with SnapToLines)
        public override IEnumerable<IChallengeConstraint> ImpliedConstraints()
        {
            foreach (var (param, start, step, end) in new[] { (XParam, XStart, XStep, XEnd), (YParam, YStart, YStep, YEnd) })
            {
                if (param == "")
                    continue;
                yield return new ValueRange { Param = param, Min = start, HasMax = true, Max = end };
                if (SnapToLines)
                    yield return new MultipleOf { Param = param, Step = step, Offset = start };
            }
        }

        public int XEnd => XStart + Columns * XStep;
        public int YEnd => YStart + Rows * YStep;

        // marker position in axis values
        public Vector2I MarkerCoordinates(ChallengeRound round) => new(
            XParam != "" ? round.Values[XParam] : RandomLine(round, "grid-x", XStart, XStep, Columns),
            YParam != "" ? round.Values[YParam] : RandomLine(round, "grid-y", YStart, YStep, Rows));

        // inner lines only (when there are any), so a random coordinate never puts the marker on the frame
        static int RandomLine(ChallengeRound round, string purpose, int start, int step, int count) =>
            start + (count > 1 ? round.RandomFor(purpose).Next(1, count) : round.RandomFor(purpose).Next(count + 1)) * step;
    }
}
