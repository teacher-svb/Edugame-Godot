
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
        ChallengeValueSelect CreateValueSelect(IMathChallenge challenge);
    }
    public class RadarUIStrategy : IChallengeUIStrategy
    {
        public ChallengeUI Build(IMathChallenge challenge)
        {
            return new ChallengeUI
                .Builder(challenge)
                .WithQuestionElement()
                .WithParamInputs(CreateParamInput, ChallengeUI.Builder.Location.ANSWER)
                .WithValueView(CreateRadarView)
                .WithSubmitButton()
                .Build();
        }

        private ChallengeParamInput CreateParamInput(IMathChallenge challenge)
        {
            throw new NotImplementedException();
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
                .WithParamInputs(CreateParamInput, ChallengeUI.Builder.Location.ANSWER)
                .WithValueView(CreateGridView)
                .WithSubmitButton()
                .Build();
        }

        private ChallengeParamInput CreateParamInput(IMathChallenge challenge)
        {
            throw new NotImplementedException();
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

        ChallengeValueSelect CreateCogwheels(IMathChallenge challenge)
        {
            ChallengeValueSelect select = new();

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

        ChallengeValueSelect CreateDropdown(IMathChallenge challenge)
        {
            ChallengeValueSelect select = new();

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
            var input = new ChallengeParamInput();

            challenge.FormulaParams
                .ForEach(p =>
                {
                    var valueInput = input.CreateChild<ChallengeValueInput>();
                    valueInput.Init(p);
                    valueInput.ValueChanged += i => input.OnParamChanged?.Invoke(p, i.ToString());
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
            var input = new ChallengeParamInput();

            var container = input.CreateChild<HBoxContainer>();
            container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            challenge.FormulaParams.ForEach(p =>
            {
                var wheel = container.CreateChild<CombinationLockWheel>();
                wheel.ValueSelected += challenge.ChangeValue;
                wheel.ValueSelected += (p, v) => input.OnParamChanged?.Invoke(p, v.ToString());
                wheel.Init(p, challenge.Values);
                wheel.ValueSelected -= challenge.ChangeValue;
            });

            return input;
        }
    }
}