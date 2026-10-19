using System;
using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Radar screen: distance rings with labels and a blip at one of the round's
    // values, for "how far away is X?" questions (e.g. formula `d`, unknown `answer`).
    [GlobalClass, Tool]
    public partial class RadarVisualization : ChallengeVisualization
    {
        [Export(PropertyHint.Range, "1,10")] public int RingCount { get; set; } = 4;

        // distance between two rings; the radar reaches RingCount * RingStep
        [Export] public int RingStep { get; set; } = 10;

        [Export] public string Unit { get; set; } = "m";

        // value that places the blip; `answer` is the formula's result
        [Export] public string DistanceParam { get; set; } = ChallengeRound.AnswerParam;

        // optional value with the blip's direction in degrees (0 = up, clockwise);
        // empty picks a direction from the round's seed
        [Export] public string AngleParam { get; set; } = "";

        // `{param}` placeholders allowed; shown next to the blip, empty for none
        [Export] public string BlipLabel { get; set; } = "";

        // ring labels sit on this diagonal (0 = up, clockwise), clear of the crosshair
        public const float LabelAngle = Mathf.Pi / 4;
        // seeded blip directions stay this far from the label diagonal, so the blip never hides a label
        const float LabelClearance = Mathf.Pi / 6;

        public int Range => RingCount * RingStep;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Radar.tscn";

        protected override IEnumerable<string> ParamFields => [nameof(DistanceParam), nameof(AngleParam)];

        // the blip has to fit on the screen
        public override IEnumerable<string> ImpliedConstraints() =>
            [$"{DistanceParam} >= 1 AND {DistanceParam} <= {Range}"];

        // blip direction in radians, 0 = up, clockwise
        public float BlipAngle(ChallengeRound round) =>
            AngleParam != ""
                ? Mathf.DegToRad(round.Values.TryGetValue(AngleParam, out var degrees) ? degrees
                    : throw new ArgumentException($"radar AngleParam '{AngleParam}' is not in formula '{round.Formula}'"))
                : LabelAngle + LabelClearance + (float)round.RandomFor("radar-angle").NextDouble() * (Mathf.Tau - 2 * LabelClearance);
    }
}
