using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TnT.EduGame.Challenges
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

        // Integer result only: fractional, non-finite or unparsable results fail. `tolerance` lets a
        // result count as whole when it is that close to one (e.g. `7 / 10 * 10` = 7.000000000000001).
        public static bool TryEvaluate(string formula, IReadOnlyDictionary<string, int> values, out int result, double tolerance = 0)
        {
            result = 0;
            if (!TryEvaluateNumber(formula, values, out var number))
                return false;

            var whole = Math.Round(number);
            if (Math.Abs(number - whole) > tolerance * Math.Max(1, Math.Abs(number)) || whole < int.MinValue || whole > int.MaxValue)
                return false;

            result = (int)whole;
            return true;
        }

        // Rejects a formula up front, with the formula in the message: bad syntax (e.g. school
        // notation `a : b` or `a × b`) or a result that isn't a number. Names are checked by the caller.
        public static void ValidateFormula(string formula, ICollection<string> knownNames)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new ArgumentException("The challenge has no formula");

            object result;
            try { result = Compute(formula, knownNames.ToDictionary(n => n, _ => 1)); }
            catch (InvalidExpressionException e)
            {
                throw new ArgumentException($"Formula '{formula}' is not a valid expression ({e.Message}); write it with + - * / % and parentheses");
            }
            // e.g. `a / (b - 1)` with the trial values; fine for others
            catch (Exception e) when (e is DivideByZeroException or OverflowException) { return; }

            if (!IsNumber(result))
                throw new ArgumentException($"Formula '{formula}' must be a calculation with a number as its result");
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

            if (!IsNumber(raw))     // null, DBNull, true/false, a string, ...
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

        static object Compute(string expression, IReadOnlyDictionary<string, int> values)
        {
            var substituted = Regex.Replace(expression, ParamPattern, m =>
                values.TryGetValue(m.Value, out var v) ? $"({v.ToString(CultureInfo.InvariantCulture)})" : m.Value);
            try
            {
                return Evaluate(substituted);
            }
            // DataTable's `%` only takes whole-number types, but its `/` always gives a Double, so
            // `(a / b) % 5` throws a type mismatch. Only then: work out every `%` in C# and evaluate
            // again. Expressions that DataTable handles itself keep exactly their old result.
            catch (Exception e) when (substituted.Contains('%') && e is EvaluateException or InvalidCastException)
            {
                var rewritten = Modulo.TryRewrite(substituted);
                if (rewritten == null)
                    throw;
                return Evaluate(rewritten);
            }
        }

        internal static object Evaluate(string expression) => new DataTable().Compute(expression, null);

        static bool IsNumber(object value) =>
            value is IConvertible c && c.GetTypeCode() is >= TypeCode.SByte and <= TypeCode.Decimal;

        internal static double ToNumber(object value) => IsNumber(value)
            ? Convert.ToDouble(value, CultureInfo.InvariantCulture)
            : throw new EvaluateException($"'{value}' is not a number");

        static bool IsEvaluationFailure(Exception e) =>
            e is InvalidExpressionException or DivideByZeroException or OverflowException or InvalidCastException;

        // Rewrites an expression so every `a % b` is replaced by its value, worked out in C#:
        // whole operands (also a Double such as `10 / 2`) give the integer remainder, as DataTable
        // does for integers; a fractional operand gives the fractional remainder (`7.5 % 5` = 2.5,
        // so `(y) % 10 = 0` still fails for a y between gridlines). A small recursive-descent
        // parser over DataTable's expression syntax, only used to find the operands of `%`: as in
        // DataTable, `%` binds looser than `*` and `/` and tighter than `+` and `-`, left-associative
        // (`a * b % c` = `(a * b) % c`, `a % b * c` = `a % (b * c)`).
        static class Modulo
        {
            static readonly Regex Token = new(
                @"\s*(\d+(?:\.\d+)?(?:[eE][+-]?\d+)?|\.\d+|[A-Za-z_][A-Za-z0-9_]*|'(?:[^']|'')*'|\[[^\]]*\]|#[^#]*#|<=|>=|<>|[-+*/%=<>(),])");

            // null when the expression doesn't parse (the caller then rethrows DataTable's error)
            public static string TryRewrite(string expression)
            {
                var tokens = new List<string>();
                int at = 0;
                while (at < expression.Length)
                {
                    if (char.IsWhiteSpace(expression[at])) { at++; continue; }
                    var m = Token.Match(expression, at);
                    if (!m.Success || m.Index != at)
                        return null;
                    tokens.Add(m.Groups[1].Value);
                    at = m.Index + m.Length;
                }

                try
                {
                    var parser = new Parser(tokens);
                    var text = parser.Or();
                    return parser.AtEnd ? text : null;
                }
                catch (FormatException) { return null; }
            }

            sealed class Parser
            {
                readonly List<string> _tokens;
                int _next;

                public Parser(List<string> tokens) => _tokens = tokens;

                public bool AtEnd => _next == _tokens.Count;
                string Peek => _next < _tokens.Count ? _tokens[_next] : null;
                bool PeekIs(params string[] options) => Peek != null && options.Any(o => string.Equals(o, Peek, StringComparison.OrdinalIgnoreCase));
                string Take() => _next < _tokens.Count ? _tokens[_next++] : throw new FormatException("unexpected end");

                void Expect(string token)
                {
                    if (Take() != token)
                        throw new FormatException($"expected {token}");
                }

                public string Or()
                {
                    var left = And();
                    while (PeekIs("OR"))
                        left = $"{left} {Take()} {And()}";
                    return left;
                }

                string And()
                {
                    var left = Not();
                    while (PeekIs("AND"))
                        left = $"{left} {Take()} {Not()}";
                    return left;
                }

                string Not() => PeekIs("NOT") ? $"{Take()} {Not()}" : Comparison();

                string Comparison()
                {
                    var left = Additive();
                    while (true)
                    {
                        if (PeekIs("=", "<>", "<", ">", "<=", ">=", "LIKE"))
                            left = $"{left} {Take()} {Additive()}";
                        else if (PeekIs("IN"))
                            left = $"{left} {Take()} {Arguments()}";
                        else if (PeekIs("IS"))
                        {
                            var op = Take();
                            if (PeekIs("NOT"))
                                op += " " + Take();
                            left = $"{left} {op} {Take()}";
                        }
                        else if (PeekIs("NOT") && _next + 1 < _tokens.Count
                                 && _tokens[_next + 1] is var after && (after.Equals("IN", StringComparison.OrdinalIgnoreCase) || after.Equals("LIKE", StringComparison.OrdinalIgnoreCase)))
                        {
                            var op = $"{Take()} {Take()}";
                            left = $"{left} {op} {(op.EndsWith("IN", StringComparison.OrdinalIgnoreCase) ? Arguments() : Additive())}";
                        }
                        else
                            return left;
                    }
                }

                string Additive()
                {
                    var left = Modulo();
                    while (PeekIs("+", "-"))
                        left = $"{left} {Take()} {Modulo()}";
                    return left;
                }

                // DataTable ranks `%` between `+ -` and `* /`: `a % b * c` = `a % (b * c)`
                string Modulo()
                {
                    var left = Multiplicative();
                    while (PeekIs("%"))
                    {
                        Take();
                        left = Remainder(left, Multiplicative());
                    }
                    return left;
                }

                string Multiplicative()
                {
                    var left = Unary();
                    while (PeekIs("*", "/"))
                        left = $"{left} {Take()} {Unary()}";
                    return left;
                }

                string Unary() => PeekIs("-", "+") ? $"{Take()} {Unary()}" : Primary();

                string Primary()
                {
                    var token = Take();
                    if (token == "(")
                    {
                        var inner = Or();
                        Expect(")");
                        return $"({inner})";
                    }
                    if (char.IsLetter(token[0]) || token[0] == '_')
                        return PeekIs("(") ? token + Arguments() : token;
                    if (char.IsDigit(token[0]) || token[0] is '.' or '\'' or '[' or '#')
                        return token;
                    throw new FormatException($"unexpected {token}");
                }

                // `(a, b, ...)` of a function call or IN
                string Arguments()
                {
                    Expect("(");
                    var args = new List<string>();
                    if (!PeekIs(")"))
                    {
                        args.Add(Or());
                        while (PeekIs(","))
                        {
                            Take();
                            args.Add(Or());
                        }
                    }
                    Expect(")");
                    return $"({string.Join(", ", args)})";
                }

                static string Remainder(string left, string right)
                {
                    var a = ToNumber(Evaluate(left));
                    var b = ToNumber(Evaluate(right));
                    // `/ 0` gives Infinity in DataTable; as a remainder that would be NaN, which DataTable
                    // can't read back ("column [NaN]"), and the validators would call it bad syntax
                    if (b == 0 || !double.IsFinite(a) || !double.IsFinite(b))
                        throw new DivideByZeroException($"{left} % {right}");

                    bool whole = a == Math.Floor(a) && b == Math.Floor(b) && Math.Abs(a) < long.MaxValue && Math.Abs(b) < long.MaxValue;
                    var text = whole
                        ? ((long)a % (long)b).ToString(CultureInfo.InvariantCulture)
                        : (a % b).ToString("0.0###############", CultureInfo.InvariantCulture);
                    return $"({text})";
                }
            }
        }
    }
}
