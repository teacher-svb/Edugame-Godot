using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TnT.EduGame.Question
{
    // Parses and evaluates challenge formulas (plain C#, via System.Data.DataTable).
    // Only whole-number results count: fractional, non-finite or unparsable results fail.
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
    }
}
