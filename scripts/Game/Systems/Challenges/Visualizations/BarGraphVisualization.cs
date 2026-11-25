using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Challenges
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

        // These four have per-class defaults (KeyVisualization overrides the Default* below). No
        // initializers on purpose: Godot reads an export's default from its initializer, the bar
        // graph's for a key too (revert went to 100, and an explicit 100 on a key wasn't saved).
        // Without one, a value is always saved, and the inspector reverts via _PropertyGetRevert.
        protected virtual int DefaultAxisMax => 100;
        protected virtual int DefaultAxisStep => 10;
        protected virtual bool DefaultSnapToGrid => true;
        protected virtual bool DefaultShowValues => false;
        int? _axisMax, _axisStep;
        bool? _snapToGrid, _showValues;

        // value axis: 0..AxisMax with a gridline every AxisStep
        [Export] public int AxisMax { get => _axisMax ?? DefaultAxisMax; set => _axisMax = value; }
        [Export] public int AxisStep { get => _axisStep ?? DefaultAxisStep; set => _axisStep = value; }

        // shown with the axis values, e.g. "L"
        [Export] public string Unit { get; set; } = "";

        // write each bar's value on top of it (makes reading trivial; off for reading practice)
        [Export] public bool ShowValues { get => _showValues ?? DefaultShowValues; set => _showValues = value; }

        // bar heights land exactly on gridlines, so they can be read off
        [Export] public bool SnapToGrid { get => _snapToGrid ?? DefaultSnapToGrid; set => _snapToGrid = value; }

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

        public override bool _PropertyCanRevert(StringName property) =>
            property == PropertyName.AxisMax || property == PropertyName.AxisStep
            || property == PropertyName.SnapToGrid || property == PropertyName.ShowValues
            || base._PropertyCanRevert(property);

        public override Variant _PropertyGetRevert(StringName property)
        {
            if (property == PropertyName.AxisMax) return DefaultAxisMax;
            if (property == PropertyName.AxisStep) return DefaultAxisStep;
            if (property == PropertyName.SnapToGrid) return DefaultSnapToGrid;
            if (property == PropertyName.ShowValues) return DefaultShowValues;
            return base._PropertyGetRevert(property);
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
