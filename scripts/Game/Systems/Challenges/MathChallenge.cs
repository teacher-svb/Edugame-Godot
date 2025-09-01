using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using TnT.Extensions;
using System.Text.RegularExpressions;
using TnT.Systems.UI;
using Godot;


namespace TnT.EduGame.Question
{
    public interface IMathChallenge
    {
        public string Name { get; }
        public string Formula { get; }
        public string Question { get; }
        public string Hint { get; }
        int ParamCount { get; }
        public string[] FormulaParams { get; }
        public bool CheckAnswer();
        public void SetFormulaParam(int index, string paramName);
        public void ChangeValue(string paramName, int value);
        public abstract Godot.Collections.Array<ChallengeValue> Values { get; }
        // TODO: remove the whole Types thing, and replace with a simple Enum
        public Type SelectedVisualType { get; }
    }

    [GlobalClass, Tool]
    public partial class MathChallenge : Resource, IMathChallenge
    {
        [Export]
        public string Name { get; set; } = "";

        [Export]
        TypeReference _visualType = new();

        [Export]
        public string Formula { get; set; } = "";

        [Export]
        public string[] FormulaParams { get; set; } = new string[0];

        [Export]
        protected string QuestionText { get; set; } = "";
        [Export]
        protected string HintText { get; set; } = "";
        public string Question
        {
            // first aggregate the question text's {paramNames} to {numbers}
            // then replace each number (e.g. {0} {1} etc) with their corresponding values
            get => FormulaParams
                    .ToList()
                    .ToDictionary(p => p, p => Values?.FirstOrDefault(v => v.ParamName == p)?.Value)
                    .Append(new("Answer", Answer))
                    .Aggregate(QuestionText, (cur, next) => cur.Replace($"{{{next.Key}}}", $"{next.Value}"));

        }
        public string Hint
        {
            get => FormulaParams
                    .ToList()
                    .ToDictionary(p => p, p => Values?.FirstOrDefault(v => v.ParamName == p)?.Value)
                    .Append(new("Answer", Answer))
                    .Aggregate(HintText, (cur, next) => cur.Replace($"{{{next.Key}}}", $"{next.Value}"));
        }
        public int ParamCount => FormulaParams.Count();

        [Export]
        public int Answer { get; private set; }
        [Export]
        public Godot.Collections.Array<ChallengeValue> Values { get; set; } = new Godot.Collections.Array<ChallengeValue>() { };
        // TODO: remove the whole Types thing, and replace with a simple Enum
        public Type SelectedVisualType => _visualType.Type;

        public MathChallenge()
        {
            ExtractParamsFromFormula();
            ComputeRandomAnswer();
        }

        [ExportToolButton("Extract Params")]
        public Callable ExtractParamsFromFormulaBtn => Callable.From(ExtractParamsFromFormula);
        public void ExtractParamsFromFormula()
        {
            if (Formula == "")
                return;

            // Extract parameter names from the formula
            var matches = Regex.Matches(Formula, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b");
            // Optionally filter out known functions/keywords or deduplicate
            FormulaParams = matches
                .Select(m => m.Value)
                .Distinct()
                .ToArray();

            if (Values == null)
                Values = new();

            while (Values.Count() < FormulaParams.Count())
                Values.Add(new());
        }

        [ExportToolButton("Generate Answer")]
        public Callable ComputeRandomAnswerBtn => Callable.From(ComputeRandomAnswer);
        public void ComputeRandomAnswer()
        {
            if (FormulaParams.Count() == 0)
                return;
            if (Values.Count() < FormulaParams.Count())
                return;

            // Replace parameter names in the formula with format placeholders
            string formattedFormula = FormulaParams
                .Select((name, index) => new { name, index })
                .Aggregate(Formula, (current, p) => current.Replace(p.name, $"{{{p.index}}}"));

            // Compute any possible answers by computing all permutations of the parameter values and evaluating the formula for each permutation
            // then select a random one from the list of possible answers
            // and store it as the answer to this challenge
            var dt = new DataTable();
            // Answer = 3;
            GD.Print(Values.Count);
            Answer = Values
                .Select(v => v.Value)
                .GetPermutations(FormulaParams.Length)
                .Select(p => p.Cast<object>().ToArray())
                .Select(p => (int)dt.Compute(string.Format(formattedFormula, p), null))
                .Distinct()
                .PickRandom();    
        }

        public bool CheckAnswer() => Evaluate(Formula, FormulaParams) == Answer;
        int Evaluate(string formula, string[] parameters)
        {
            if (parameters.Length != Values.Where(v => v.ParamName != "").Count())
                throw new ChallengeParametersMissingException();

            // 1. Replace each variable name with its numeric literal
            foreach (var param in parameters)
                formula = formula.Replace(param, Values.FirstOrDefault(v => v.ParamName == param).Value.ToString());

            // 2. Let DataTable do the math
            var dt = new DataTable();
            return (int)dt.Compute(formula, null);
        }

        public void SetFormulaParam(int index, string paramName)
        {
            // reset any value that currently is mapped to the param name
            this.Values.ToList().FindAll(v => v.ParamName == paramName).ForEach(v => v.ParamName = "");
            // map the new parameter name to the index-th value in the list of values
            Values.ElementAt(index).ParamName = paramName;
        }

        public void ChangeValue(string paramName, int value)
        {
            this.Values.ToList().FindAll(v => v.ParamName == paramName).ForEach(v => v.Value = value);
        }

        public class ChallengeParametersMissingException : Exception { }
    }
}
