
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CombinationLockValue : Label
    {
        public int Value { get; set; }

        public CombinationLockValue(ChallengeValue value)
        {
            Value = value.Value;
            this.Text = value.Value.ToString();
        }
    }

    public partial class CombinationLockWheel : Control
    {
        public Action<string, int> OnValueSelected;
        string _paramName;
        Control _content;
        Button _btnDown;
        Button _btnUp;

        public CombinationLockWheel(string paramName, IEnumerable<ChallengeValue> values)
        {
            _paramName = paramName;
            _content = this.CreateChild<Control>("CombinationLockWheel");

            values
                .Select(v => new CombinationLockValue(v))
                .ToList()
                .ForEach(v => _content.AddChild(v));

            var nav = this.CreateChild<Control>("CombinationLockWheel_nav");
            _btnUp = nav.CreateChild<Button>("CombinationLockWheel_up");
            _btnUp.Text = "up";
            _btnDown = nav.CreateChild<Button>("CombinationLockWheel_down");
            _btnDown.Text = "down";

            _btnUp.Pressed += GoLeft;
            _btnDown.Pressed += GoRight;
        }

        void GoRight()
        {
            if (_btnDown.Disabled == false)
                _ = MoveRight();
        }

        async Task MoveRight()
        {
            _btnDown.Disabled = true;
            Reorder(1);
            // var next = _content.Children().ElementAt(0);
            // _content.RemoveAt(0);
            // _content.Add(next);
            _btnDown.Disabled = false;
            await Task.Yield();
        }

        void GoLeft()
        {
            if (_btnUp.Disabled == false)
                _ = MoveLeft();
        }

        async Task MoveLeft()
        {
            _btnUp.Disabled = true;
            Reorder(-1);
            // var next = _content.Children().ElementAt(_content.childCount - 1);
            // _content.RemoveAt(_content.childCount - 1);
            // _content.Insert(0, next);
            _btnUp.Disabled = false;
            await Task.Yield();
        }

        void Reorder(int offset = 0)
        {
            // var prev = _content.Children().FirstOrDefault(c => c.ClassListContains("previous"));
            // var curr = _content.Children().FirstOrDefault(c => c.ClassListContains("current")) as CombinationLockValue;
            // var next = _content.Children().FirstOrDefault(c => c.ClassListContains("next"));

            // prev?.RemoveFromClassList("previous");
            // curr?.RemoveFromClassList("current");
            // next?.RemoveFromClassList("next");

            // int currId = (_content.childCount / 2) + offset;
            // int prevId = currId - 1;
            // int nextId = currId + 1;

            // prev = _content.Children().ElementAtOrDefault(prevId);
            // prev?.AddToClassList("previous");

            // curr = _content.Children().ElementAtOrDefault(currId) as CombinationLockValue;
            // curr?.AddToClassList("current");

            // next = _content.Children().ElementAtOrDefault(nextId);
            // next?.AddToClassList("next");

            // OnValueSelected.Invoke(_paramName, curr.Value);
        }
    }
}