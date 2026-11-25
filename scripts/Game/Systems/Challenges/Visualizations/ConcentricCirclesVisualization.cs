using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace TnT.EduGame.Challenges
{
    // Concentric rings, read outer → inner: numbers and operators alternate, each
    // ring spins to its option and locks in at the top, the centre shows the
    // result. `x + y - z` gives five rings: x, +, y, -, z.
    //
    // With a non-empty Operators pool every operator slot in the formula is
    // replaced by a seeded pick from it, so the round's formula (and thus the
    // answer check) differs per round. Parentheses aren't supported: rings can't
    // show grouping, and the result follows normal operator precedence.
    [GlobalClass, Tool]
    public partial class ConcentricCirclesVisualization : ChallengeVisualization
    {
        // e.g. ["+", "-", "*"]; empty keeps the formula's own operators
        [Export] public string[] Operators { get; set; } = [];

        // segments shown around each number ring (the correct one included)
        [Export(PropertyHint.Range, "2,12")] public int NumberSegments { get; set; } = 6;

        // seconds the outer ring spins; inner rings land later, one by one
        [Export] public float SpinDuration { get; set; } = 1.4f;

        const int MaxOperatorAttempts = 20;
        static readonly string[] DefaultOperatorSegments = ["+", "-", "*", "/"];

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/ConcentricCircles.tscn";

        public enum RingKind { Number, Operator }

        // What a ring shows: its segments and which one ends up at the top.
        public record Ring(RingKind Kind, IReadOnlyList<string> Segments, int SelectedIndex);

        public override ChallengeRound Generate(MathChallenge challenge, int seed)
        {
            var tokens = Tokenize(challenge.Formula);
            if (Operators is not { Length: > 0 })
                return base.Generate(challenge, seed);

            var rng = ChallengeRound.RandomFor(seed, "operators");
            for (int attempt = 0; attempt < MaxOperatorAttempts; attempt++)
            {
                var formula = string.Join(" ", tokens.Select(t => IsOperator(t) ? Operators[rng.Next(Operators.Length)] : t));
                try
                {
                    return ChallengeRound.Roll(formula, challenge.Params, challenge.Unknowns, seed, ConstraintsFor(challenge));
                }
                catch (InvalidOperationException)
                {
                    // e.g. a division that never divides evenly in the param ranges: pick other operators
                }
            }

            throw new InvalidOperationException(
                $"{challenge.Name}: no operator combination from [{string.Join(" ", Operators)}] gives a valid round for '{challenge.Formula}'");
        }

        // Rings for a rolled round, outer first. Unknown params lock in on `?`.
        public IReadOnlyList<Ring> BuildRings(ChallengeRound round)
        {
            var operatorSegments = (Operators is { Length: > 0 } ? Operators : DefaultOperatorSegments).Distinct().ToList();

            return Tokenize(round.Formula).Select(token =>
            {
                if (IsOperator(token))
                {
                    var segments = operatorSegments.Contains(token) ? operatorSegments : operatorSegments.Append(token).ToList();
                    return new Ring(RingKind.Operator, segments.Select(Prettify).ToList(), segments.IndexOf(token));
                }

                if (!round.Values.ContainsKey(token))
                    return new Ring(RingKind.Number, [token], 0); // literal constant in the formula

                var options = round.OptionsFor(token, NumberSegments).ToList();
                var selected = options.IndexOf(round.Values[token]);
                var labels = options.Select(o => o.ToString()).ToList();
                if (round.IsUnknown(token))
                    labels[selected] = "?";
                return new Ring(RingKind.Number, labels, selected);
            }).ToList();
        }

        // Alternating number and operator tokens, number first and last; anything a ring can't
        // show is an error with the formula in the message (before: silently dropped, so `2.5`
        // became two rings and a unary minus an operator ring without a number in front).
        public static IReadOnlyList<string> Tokenize(string formula)
        {
            formula ??= "";
            if (formula.IndexOfAny(['(', ')']) >= 0)
                throw new ArgumentException($"Concentric circles can't show parentheses: '{formula}'");

            var tokens = Regex.Matches(formula, @"[a-zA-Z_][a-zA-Z0-9_]*|\d+(?:\.\d+)?|[+\-*/]|\S")
                .Select(m => m.Value)
                .ToList();

            var foreign = tokens.FirstOrDefault(t => !IsOperator(t) && !Regex.IsMatch(t, @"^([a-zA-Z_][a-zA-Z0-9_]*|\d+(\.\d+)?)$"));
            if (foreign != null)
                throw new ArgumentException($"Concentric circles can't show '{foreign}' in '{formula}'; use + - * / between numbers and params");

            for (int i = 0; i < tokens.Count; i++)
                if (IsOperator(tokens[i]) != (i % 2 == 1))
                    throw new ArgumentException(i == 0 && IsOperator(tokens[i])
                        ? $"Concentric circles can't show a leading '{tokens[i]}' in '{formula}'; write e.g. '0 - a'"
                        : $"Concentric circles need a number or param between operators in '{formula}'");
            if (tokens.Count % 2 == 0)
                throw new ArgumentException($"Concentric circles need a number or param after the last operator in '{formula}'");

            return tokens;
        }

        static bool IsOperator(string token) => token is "+" or "-" or "*" or "/";

        // school notation rather than programmer notation
        public static string Prettify(string op) => op switch { "*" => "×", "/" => ":", _ => op };
    }
}
