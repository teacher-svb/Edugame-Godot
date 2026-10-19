using System;
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

        const int MaxRangeAttempts = 50;
        // large prime, spreads retry seeds apart
        const int SeedStride = 104729;

        public int Range => RingCount * RingStep;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Radar.tscn";

        // reroll (deterministically) until the blip fits on the screen
        public override ChallengeRound Generate(MathChallenge challenge, int seed)
        {
            for (int attempt = 0; attempt < MaxRangeAttempts; attempt++)
            {
                var round = base.Generate(challenge, unchecked(seed + attempt * SeedStride));
                if (!round.Values.TryGetValue(DistanceParam, out var distance))
                    throw new ArgumentException($"{challenge.Name}: radar DistanceParam '{DistanceParam}' is not in the formula");
                if (AngleParam != "" && !round.Values.ContainsKey(AngleParam))
                    throw new ArgumentException($"{challenge.Name}: radar AngleParam '{AngleParam}' is not in the formula");

                if (distance > 0 && distance <= Range)
                    return round;
            }

            throw new InvalidOperationException(
                $"{challenge.Name}: '{DistanceParam}' never lands within the radar range (1..{Range}); check the ParamSpec ranges");
        }

        // blip direction in radians, 0 = up, clockwise
        public float BlipAngle(ChallengeRound round) =>
            AngleParam != ""
                ? Mathf.DegToRad(round.Values[AngleParam])
                : LabelAngle + LabelClearance + (float)round.RandomFor("radar-angle").NextDouble() * (Mathf.Tau - 2 * LabelClearance);
    }
}
