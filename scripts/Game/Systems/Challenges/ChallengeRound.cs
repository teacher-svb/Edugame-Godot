using System;
using System.Collections.Generic;
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
    // Deliberately free of Godot types (ParamSpec aside) so the math stays testable;
    // parsing and evaluating formulas lives in FormulaEvaluator.
    public class ChallengeRound
    {
        public const string AnswerParam = "answer";

        // strict rules (e.g. a grid snap on two axes passes ~1 in 100 rolls) need plenty of tries
        const int MaxRollAttempts = 1000;
        // answer distractors: start this far around the correct value, widen until enough pass the rules
        const int MinDistractorSpread = 5;
        const int MaxDistractorSpread = 10000;
        const int DefaultOptionCount = 4;
        public int Seed { get; }
        public string Formula { get; }
        // every param plus `answer`
        public IReadOnlyDictionary<string, int> Values { get; }
        public IReadOnlyList<string> Unknowns { get; }
        public IReadOnlyDictionary<string, int> Givens { get; }

        readonly Dictionary<string, ParamSpec> _specs;
        readonly IReadOnlyList<IChallengeConstraint> _constraints;

        public ChallengeRound(int seed, string formula, IReadOnlyDictionary<string, int> values,
            IEnumerable<string> unknowns, IEnumerable<ParamSpec> specs, IEnumerable<IChallengeConstraint> constraints = null)
        {
            Seed = seed;
            Formula = formula;
            Values = new Dictionary<string, int>(values);
            Unknowns = unknowns.Distinct().ToList();
            _specs = specs.Where(s => s != null).GroupBy(s => s.Name).ToDictionary(g => g.Key, g => g.First());
            _constraints = (constraints ?? []).Where(c => c != null).ToList();

            var undefined = Unknowns.Where(u => !Values.ContainsKey(u)).ToList();
            if (undefined.Any())
                throw new ArgumentException($"Unknowns not present in formula '{formula}': {string.Join(", ", undefined)}");

            Givens = Values.Where(kv => !Unknowns.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public bool IsUnknown(string param) => Unknowns.Contains(param);

        // Rolls a value for every spec and computes `answer`, retrying (with the
        // same seeded rng, so still deterministic) until the answer is an integer
        // and every constraint holds.
        public static ChallengeRound Roll(string formula, IEnumerable<ParamSpec> specs, IEnumerable<string> unknowns,
            int seed, IEnumerable<IChallengeConstraint> constraints = null)
        {
            var specList = specs.Where(s => s != null).ToList();
            var constraintList = (constraints ?? []).Where(c => c != null).ToList();

            if (FormulaEvaluator.ExtractParamNames(formula).Contains(AnswerParam))
                throw new ArgumentException($"Formula '{formula}' must not reference '{AnswerParam}'; it is the formula's result");

            var unspecified = FormulaEvaluator.ExtractParamNames(formula).Except(specList.Select(s => s.Name)).ToList();
            if (unspecified.Any())
                throw new ArgumentException($"Formula '{formula}' has params without a ParamSpec: {string.Join(", ", unspecified)}");

            var known = specList.Select(s => s.Name).Append(AnswerParam).ToHashSet();
            foreach (var constraint in constraintList)
            {
                var foreign = constraint.Params.Where(p => !known.Contains(p)).ToList();
                if (foreign.Any())
                    throw new ArgumentException($"Constraint '{constraint.Describe()}' refers to {string.Join(", ", foreign)}, which is not in formula '{formula}'");
            }

            var rng = RandomFor(seed, "roll");
            for (int attempt = 0; attempt < MaxRollAttempts; attempt++)
            {
                var values = specList.ToDictionary(s => s.Name, s => Sample(s, rng));
                if (!FormulaEvaluator.TryEvaluate(formula, values, out var answer))
                    continue;

                values[AnswerParam] = answer;
                if (constraintList.All(c => c.IsSatisfiedBy(values)))
                    return new ChallengeRound(seed, formula, values, unknowns, specList, constraintList);
            }

            var rules = constraintList.Any() ? $" with {string.Join(", ", constraintList.Select(c => c.Describe()))}" : "";
            throw new InvalidOperationException(
                $"Could not roll a valid round for '{formula}'{rules} in {MaxRollAttempts} attempts (seed {seed}); check the ParamSpec ranges and constraints");
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

            return FormulaEvaluator.TryEvaluate(Formula, all, out var result) && result == all[AnswerParam];
        }

        // Values an input widget may offer for `param`: the authored pool if there
        // is one, otherwise the correct value plus seeded distractors.
        public IReadOnlyList<int> OptionsFor(string param, int count = DefaultOptionCount)
        {
            if (!Values.TryGetValue(param, out var correct))
                throw new ArgumentException($"Unknown param '{param}'");

            var rng = RandomFor($"options:{param}");
            _specs.TryGetValue(param, out var spec);

            if (spec?.Pool is { Length: > 0 })
                return spec.Pool.Distinct().OrderBy(_ => rng.Next()).ToList();

            List<int> candidates;
            if (spec is { IsFixed: false })
                candidates = Enumerable.Range(Math.Min(spec.Min, spec.Max), Math.Abs(spec.Max - spec.Min) + 1)
                    .Where(c => c != correct && Allows(param, c)).ToList();
            else
            {
                // answer or fixed param: no range to draw from, so look around the correct value,
                // widening until enough candidates pass the rules (e.g. only grid lines)
                var spread = Math.Max(MinDistractorSpread, Math.Abs(correct) / 4);
                do
                {
                    candidates = Enumerable.Range(correct - spread, spread * 2 + 1)
                        .Where(c => c != correct && Allows(param, c)).ToList();
                    spread *= 2;
                } while (candidates.Count < count - 1 && spread <= MaxDistractorSpread);
            }

            return candidates
                .OrderBy(_ => rng.Next())
                .Take(count - 1)
                .Append(correct)
                .OrderBy(_ => rng.Next())
                .ToList();
        }

        // would the round still satisfy every rule with `param` set to `value`?
        bool Allows(string param, int value)
        {
            var values = new Dictionary<string, int>(Values) { [param] = value };
            return _constraints.All(c => c.IsSatisfiedBy(values));
        }

        // Independent random stream per purpose (e.g. "radar-angle"), so rolling,
        // options and visualizations each draw their own replayable randomness from the seed.
        public Random RandomFor(string purpose) => RandomFor(Seed, purpose);

        public static Random RandomFor(int seed, string purpose) => new(Mix(unchecked(seed * 31 + StableHash(purpose))));

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

        static int Sample(ParamSpec spec, Random rng)
        {
            if (spec.IsFixed)
                return spec.FixedValue;
            if (spec.Pool is { Length: > 0 })
                return spec.Pool[rng.Next(spec.Pool.Length)];
            return rng.Next(Math.Min(spec.Min, spec.Max), Math.Max(spec.Min, spec.Max) + 1);
        }

        // System.Random's first value barely differs between neighbouring seeds; scramble first
        static int Mix(int seed)
        {
            unchecked
            {
                var x = (uint)seed * 0x9E3779B9u;
                x ^= x >> 16;
                x *= 0x85EBCA6Bu;
                x ^= x >> 13;
                return (int)x;
            }
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
