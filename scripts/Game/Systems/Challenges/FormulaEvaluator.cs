using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TnT.EduGame.Question
{
    // Parses and evaluates challenge formulas and rules (plain C#, via System.Data.DataTable).
    // Formulas only count with a whole-number result; rules are true/false expressions in the
    // same language, e.g. `answer >= 0`, `x % y = 0`, `x > y AND x < 10`, `distinct(x, y, z)`.
    public static class FormulaEvaluator
    {
        // a param name: identifier-like token
        public const string ParamPattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b";

        // DataTable expression keywords/functions that the param regex would otherwise pick up
        static readonly HashSet<string> ExpressionKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or", "not", "true", "false", "null", "like", "in", "is",
            "iif", "isnull", "len", "trim", "substring", "convert",
        };

        public static string[] ExtractParamNames(string formula) =>
            Regex.Matches(formula ?? "", ParamPattern)
                .Select(m => m.Value)
                .Where(name => !ExpressionKeywords.Contains(name))
                .Distinct()
                .ToArray();

        // distinct(a, b, ...) → (a <> b AND a <> c AND ...)
        static readonly Regex DistinctShorthand = new(@"\bdistinct\s*\(([^()]*)\)", RegexOptions.IgnoreCase);

        // Integer result only: fractional, non-finite or unparsable results fail.
        public static bool TryEvaluate(string formula, IReadOnlyDictionary<string, int> values, out int result)
        {
            result = 0;
            if (!TryEvaluateNumber(formula, values, out var number) || number != Math.Floor(number)
                || number < int.MinValue || number > int.MaxValue)
                return false;

            result = (int)number;
            return true;
        }

        // Any finite number (e.g. a graph point between gridlines); unparsable or non-numeric fails.
        public static bool TryEvaluateNumber(string expression, IReadOnlyDictionary<string, int> values, out double result)
        {
            result = 0;
            object raw;
            try { raw = Compute(expression, values); }
            catch (Exception e) when (IsEvaluationFailure(e))
            {
                return false;
            }

            if (raw is null or DBNull or bool)
                return false;

            result = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
            return !double.IsNaN(result) && !double.IsInfinity(result);
        }

        // Rule shorthands rewritten into plain expressions; apply once before Holds/ValidateRule.
        public static string ExpandRule(string rule) =>
            DistinctShorthand.Replace(rule ?? "", m =>
            {
                var names = m.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var pairs = names.SelectMany((a, i) => names.Skip(i + 1).Select(b => $"{a} <> {b}")).ToList();
                return pairs.Any() ? $"({string.Join(" AND ", pairs)})" : "true";
            });

        // Does the (expanded) rule hold for these values? A rule that can't be evaluated
        // for them (e.g. `% 0`) counts as not holding, so the round is rerolled.
        public static bool Holds(string rule, IReadOnlyDictionary<string, int> values)
        {
            try { return Compute(rule, values) is true; }
            catch (Exception e) when (IsEvaluationFailure(e)) { return false; }
        }

        // Rejects a rule up front, with the rule in the message: unknown names, bad
        // syntax, or an expression that isn't a true/false comparison.
        public static void ValidateRule(string rule, string expanded, ICollection<string> knownNames)
        {
            var foreign = ExtractParamNames(expanded).Where(n => !knownNames.Contains(n)).ToList();
            if (foreign.Any())
                throw new ArgumentException($"Rule '{rule}' refers to {string.Join(", ", foreign)}, which is not in the formula");

            object result;
            try { result = Compute(expanded, knownNames.ToDictionary(n => n, _ => 1)); }
            catch (InvalidExpressionException e) { throw new ArgumentException($"Rule '{rule}' is not a valid expression: {e.Message}"); }
            // e.g. `x % (y - 1)` with the trial values; fine for others
            catch (Exception e) when (e is DivideByZeroException or OverflowException) { return; }

            if (result is not bool)
                throw new ArgumentException($"Rule '{rule}' must be a comparison that is true or false, e.g. 'answer >= 0'");
        }

        static object Compute(string expression, IReadOnlyDictionary<string, int> values) =>
            new DataTable().Compute(Regex.Replace(expression, ParamPattern, m =>
                values.TryGetValue(m.Value, out var v) ? $"({v.ToString(CultureInfo.InvariantCulture)})" : m.Value), null);

        static bool IsEvaluationFailure(Exception e) =>
            e is InvalidExpressionException or DivideByZeroException or OverflowException or InvalidCastException;
    }
}
