using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Question
{
    // Bar chart drawn from the round's values: one bar per GraphBar, heights read off a
    // labelled value axis. A bar whose param is unknown is drawn as an outline with `?`.
    // E.g. bars `a`, `b`, `c` for each family member's water use, formula `a + b + c`.
    [GlobalClass, Tool]
    public partial class BarGraphVisualization : ChallengeVisualization
    {
        [Export]
        public Godot.Collections.Array<GraphBar> Bars
        {
            get => _bars;
            set { _bars = value; ShareParamsWithBars(); }
        }
        Godot.Collections.Array<GraphBar> _bars = new();

        // value axis: 0..AxisMax with a gridline every AxisStep
        [Export] public int AxisMax { get; set; } = 100;
        [Export] public int AxisStep { get; set; } = 10;

        // shown with the axis values, e.g. "L"
        [Export] public string Unit { get; set; } = "";

        // write each bar's value on top of it (makes reading trivial; off for reading practice)
        [Export] public bool ShowValues { get; set; }

        // bar heights land exactly on gridlines, so they can be read off
        [Export] public bool SnapToGrid { get; set; } = true;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/BarGraph.tscn";

        IEnumerable<string> BarParams => (_bars ?? new()).Where(b => b != null && b.Param != "").Select(b => b.Param);

        // Snapping by rerolling would pass ~1 in AxisStep rolls per bar, so ranged bar params
        // are narrowed to gridline values before rolling instead (also makes their input options gridlines).
        public override ChallengeRound Generate(MathChallenge challenge, int seed) =>
            ChallengeRound.Roll(challenge.Formula, challenge.Params.Select(SnapToGridSpec), challenge.Unknowns, seed, ConstraintsFor(challenge));

        public override IEnumerable<string> ImpliedConstraints()
        {
            foreach (var param in BarParams)
            {
                yield return $"{param} >= 0 AND {param} <= {AxisMax}";
                if (SnapToGrid && AxisStep > 0)
                    yield return $"{param} % {AxisStep} = 0";
            }
        }

        ParamSpec SnapToGridSpec(ParamSpec spec)
        {
            if (spec == null || spec.IsFixed || spec.Pool is { Length: > 0 } || !SnapToGrid || AxisStep <= 0 || !BarParams.Contains(spec.Name))
                return spec;

            var low = Mathf.Max(Mathf.Min(spec.Min, spec.Max), 0);
            var high = Mathf.Min(Mathf.Max(spec.Min, spec.Max), AxisMax);
            var gridValues = Enumerable.Range(low, Mathf.Max(high - low + 1, 0)).Where(v => v % AxisStep == 0).ToArray();

            // nothing on the grid in range: keep the spec, the implied rules then report the problem
            return gridValues.Length == 0 ? spec : new ParamSpec(spec.Name) { Pool = gridValues };
        }

        public override void SetAvailableParams(string[] names)
        {
            base.SetAvailableParams(names);
            ShareParamsWithBars();
        }

        void ShareParamsWithBars()
        {
            foreach (var bar in _bars ?? new())
                bar?.SetAvailableParams(AvailableParams);
        }
    }
}
