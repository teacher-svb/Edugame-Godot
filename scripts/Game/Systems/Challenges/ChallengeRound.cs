using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TnT.EduGame.Question
{
    // One concrete, rolled instance of a MathChallenge: every formula param plus
    // `answer` has a value, and Unknowns lists the ones the player must supply.
    // All values are rolled up front and only then hidden, so a round always has
    // at least one integer solution. Deterministic for a given seed, so a failed
    // attempt can be replayed with the exact same values.
    //
    // Deliberately free of Godot types (ParamSpec aside) so the math stays testable.
    public class ChallengeRound
    {
        public const string AnswerParam = "answer";

        const int MaxRollAttempts = 100;
        const int DefaultOptionCount = 4;
        const string ParamPattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b";

        // DataTable expression keywords/functions that the param regex would otherwise pick up
        static readonly HashSet<string> ExpressionKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or", "not", "true", "false", "null", "like", "in", "is",
            "iif", "isnull", "len", "trim", "substring", "convert",
        };

        public int Seed { get; }
        public string Formula { get; }
        // every param plus `answer`
        public IReadOnlyDictionary<string, int> Values { get; }
        public IReadOnlyList<string> Unknowns { get; }
        public IReadOnlyDictionary<string, int> Givens { get; }

        readonly Dictionary<string, ParamSpec> _specs;
        readonly bool _allowNegativeAnswer;
        readonly bool _distinctValues;

        public ChallengeRound(int seed, string formula, IReadOnlyDictionary<string, int> values,
            IEnumerable<string> unknowns, IEnumerable<ParamSpec> specs, bool allowNegativeAnswer = false, bool distinctValues = false)
        {
            Seed = seed;
            Formula = formula;
            Values = new Dictionary<string, int>(values);
            Unknowns = unknowns.Distinct().ToList();
            _specs = specs.Where(s => s != null).GroupBy(s => s.Name).ToDictionary(g => g.Key, g => g.First());
            _allowNegativeAnswer = allowNegativeAnswer;
            _distinctValues = distinctValues;

            var undefined = Unknowns.Where(u => !Values.ContainsKey(u)).ToList();
            if (undefined.Any())
                throw new ArgumentException($"Unknowns not present in formula '{formula}': {string.Join(", ", undefined)}");

            Givens = Values.Where(kv => !Unknowns.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public bool IsUnknown(string param) => Unknowns.Contains(param);

        // Rolls a value for every spec and computes `answer`, retrying (with the
        // same seeded rng, so still deterministic) until the answer is an integer
        // and, unless allowed, non-negative. With distinctValues no two params share a value.
        public static ChallengeRound Roll(string formula, IEnumerable<ParamSpec> specs, IEnumerable<string> unknowns,
            int seed, bool allowNegativeAnswer = false, bool distinctValues = false)
        {
            var specList = specs.Where(s => s != null).ToList();

            if (ExtractParamNames(formula).Contains(AnswerParam))
                throw new ArgumentException($"Formula '{formula}' must not reference '{AnswerParam}'; it is the formula's result");

            var unspecified = ExtractParamNames(formula).Except(specList.Select(s => s.Name)).ToList();
            if (unspecified.Any())
                throw new ArgumentException($"Formula '{formula}' has params without a ParamSpec: {string.Join(", ", unspecified)}");

            var rng = new Random(seed);
            for (int attempt = 0; attempt < MaxRollAttempts; attempt++)
            {
                var values = specList.ToDictionary(s => s.Name, s => Sample(s, rng));
                if (distinctValues && !AreDistinct(values))
                    continue;

                if (TryEvaluate(formula, values, out var answer) && (allowNegativeAnswer || answer >= 0))
                {
                    values[AnswerParam] = answer;
                    return new ChallengeRound(seed, formula, values, unknowns, specList, allowNegativeAnswer, distinctValues);
                }
            }

            throw new InvalidOperationException(
                $"Could not roll a valid round for '{formula}' in {MaxRollAttempts} attempts (seed {seed}); check the ParamSpec ranges");
        }

        // Correct as soon as the player's values satisfy the formula, which need
        // not be the rolled values (e.g. any three numbers that sum to `answer`).
        public bool Check(IReadOnlyDictionary<string, int> playerValues)
        {
            var missing = Unknowns.Where(u => !playerValues.ContainsKey(u)).ToList();
            if (missing.Any())
                throw new InvalidOperationException($"No value supplied for: {string.Join(", ", missing)}");

            var all = new Dictionary<string, int>(Givens);
            foreach (var unknown in Unknowns)
                all[unknown] = playerValues[unknown];

            if (_distinctValues && !AreDistinct(all.Where(kv => kv.Key != AnswerParam).ToDictionary(kv => kv.Key, kv => kv.Value)))
                return false;

            return TryEvaluate(Formula, all, out var result) && result == all[AnswerParam];
        }

        // Values an input widget may offer for `param`: the authored pool if there
        // is one, otherwise the correct value plus seeded distractors.
        public IReadOnlyList<int> OptionsFor(string param, int count = DefaultOptionCount)
        {
            if (!Values.TryGetValue(param, out var correct))
                throw new ArgumentException($"Unknown param '{param}'");

            var rng = new Random(unchecked(Seed * 31 + StableHash(param)));
            _specs.TryGetValue(param, out var spec);

            if (spec?.Pool is { Length: > 0 })
                return spec.Pool.Distinct().OrderBy(_ => rng.Next()).ToList();

            IEnumerable<int> candidates;
            if (spec is { IsFixed: false })
                candidates = Enumerable.Range(Math.Min(spec.Min, spec.Max), Math.Abs(spec.Max - spec.Min) + 1);
            else
            {
                // answer or fixed param: no range to draw from, so spread around the correct value
                var spread = Math.Max(5, Math.Abs(correct) / 4);
                candidates = Enumerable.Range(correct - spread, spread * 2 + 1);
            }

            if (param == AnswerParam && !_allowNegativeAnswer)
                candidates = candidates.Where(c => c >= 0);

            return candidates
                .Where(c => c != correct)
                .OrderBy(_ => rng.Next())
                .Take(count - 1)
                .Append(correct)
                .OrderBy(_ => rng.Next())
                .ToList();
        }

        // Fills `{param}` placeholders with the round's values; unknowns become
        // `unknownPlaceholder`. Exact name match first, then case-insensitive, so
        // `{Answer}` in older resources still resolves.
        public string Format(string template, string unknownPlaceholder = "?") =>
            Regex.Replace(template ?? "", @"\{(\w+)\}", m =>
            {
                var name = m.Groups[1].Value;
                var key = Values.Keys.FirstOrDefault(k => k == name)
                    ?? Values.Keys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));

                if (key == null)
                    return m.Value;
                return IsUnknown(key) ? unknownPlaceholder : Values[key].ToString(CultureInfo.InvariantCulture);
            });

        public static string[] ExtractParamNames(string formula) =>
            Regex.Matches(formula ?? "", ParamPattern)
                .Select(m => m.Value)
                .Where(name => !ExpressionKeywords.Contains(name))
                .Distinct()
                .ToArray();

        // Integer result only: fractional, non-finite or unparsable results fail.
        public static bool TryEvaluate(string formula, IReadOnlyDictionary<string, int> values, out int result)
        {
            result = 0;
            var expression = Regex.Replace(formula, ParamPattern, m =>
                values.TryGetValue(m.Value, out var v) ? $"({v.ToString(CultureInfo.InvariantCulture)})" : m.Value);

            object raw;
            try { raw = new DataTable().Compute(expression, null); }
            catch (Exception e) when (e is EvaluateException or SyntaxErrorException or DivideByZeroException or OverflowException)
            {
                return false;
            }

            if (raw is null or DBNull)
                return false;

            var number = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number) || number != Math.Floor(number)
                || number < int.MinValue || number > int.MaxValue)
                return false;

            result = (int)number;
            return true;
        }

        static bool AreDistinct(IReadOnlyDictionary<string, int> values) =>
            values.Values.Distinct().Count() == values.Count;

        static int Sample(ParamSpec spec, Random rng)
        {
            if (spec.IsFixed)
                return spec.FixedValue;
            if (spec.Pool is { Length: > 0 })
                return spec.Pool[rng.Next(spec.Pool.Length)];
            return rng.Next(Math.Min(spec.Min, spec.Max), Math.Max(spec.Min, spec.Max) + 1);
        }

        // string.GetHashCode is randomized per process; seeds must survive save/load
        static int StableHash(string s)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (var c in s)
                    hash = (hash ^ c) * 16777619;
                return (int)hash;
            }
        }
    }
}
