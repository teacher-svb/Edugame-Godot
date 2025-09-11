
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public enum ChallengeUIType
    {
        CogwheelUIStrategy,
        RadarUIStrategy,
        SearchGridUIStrategy,
        DropdownUIStrategy,
        TextInputUIStrategy,
        CombinationLockUIStrategy
    }

    public interface IChallengeUIStrategy
    {
        ChallengeUI Build(IMathChallenge challenge);
    }
    public interface IValueSelectProvider
    {
        ChallengeSelect CreateValueSelect(IMathChallenge challenge);
    }
    public class RadarUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithAnswerInput()
                .WithValueView(CreateRadarView)
                .WithSubmitButton()
                .Build();
        }

        private ChallengeValueView CreateRadarView(IMathChallenge challenge)
        {
            throw new NotImplementedException();
        }
    }

    public class SearchGridUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithAnswerInput()
                .WithValueView(CreateGridView)
                .WithSubmitButton()
                .Build();
        }

        private ChallengeValueView CreateGridView(IMathChallenge challenge)
        {
            throw new NotImplementedException();
        }
    }

    public class CogwheelUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithValueSelect(CreateCogwheels)
                .WithSubmitButton()
                .Build();
        }

        ChallengeSelect CreateCogwheels(IMathChallenge challenge)
        {
            ChallengeSelect select = new();

            challenge.Values
                .Select((v, i) => new ChallengeSelectOption(i, v.ParamName, v.Value))
                .Shuffle(42) // just a predetermined seed, so shuffling happens to the same order every time
                .ToList()
                .ForEach(o =>
                {
                    o.Pressed += () => select.OnValueSelected?.Invoke(o.Index);
                    o.AddTo(select);
                });

            return select;
        }
    }

    public class DropdownUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithValueSelect(CreateDropdown)
                .WithSubmitButton()
                .Build();
        }

        ChallengeSelect CreateDropdown(IMathChallenge challenge)
        {
            ChallengeSelect select = new();

            var dropdown = select.CreateChild<OptionButton>();
            challenge.Values.Select(v => v.Value.ToString()).ForEach(t => dropdown.AddItem(t));
            dropdown
            .ItemSelected += c =>
                    select
                        .OnValueSelected?
                        .Invoke(challenge.Values.ToList().FindIndex(v => v.Value == c));

            return select;
        }
    }

    public class TextInputUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithParamInputs(CreateTextInputs)
                .WithSubmitButton()
                .Build();
        }

        ChallengeParamInput CreateTextInputs(IMathChallenge challenge)
        {
            ChallengeParamInput input = new();

            challenge.FormulaParams
                .Select(p => new ChallengeValueInput(p))
                .ToList()
                .ForEach(p =>
                {
                    p.TextChanged += i => input.OnParamChanged?.Invoke(p.ParamName, i);
                    p.AddTo(input);
                });

            return input;
        }
    }

    public class CombinationLockUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithParamInputs(CreateCombinationLock)
                .WithSubmitButton()
                .Build();
        }

        private ChallengeParamInput CreateCombinationLock(IMathChallenge challenge)
        {
            ChallengeParamInput input = new();

            challenge.FormulaParams
                .Select(p => new CombinationLockWheel(p, challenge.Values))
                .ToList()
                .ForEach(w =>
                {

                    w.OnValueSelected += (p, v) => input.OnParamChanged?.Invoke(p, v);
                    input.AddChild(w);
                    w.AddTo(input);
                });

            return input;
        }
    }
}