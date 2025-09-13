
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CombinationLock : Control
    {
        public Action<string, string> OnValueSelected;
        List<CombinationLockWheel> _wheels;

        public void Init(string[] paramNames, IEnumerable<ChallengeValue> values)
        {
            var container = this.CreateChild<HBoxContainer>();
            _wheels = paramNames.Select(p =>
            {
                var wheel = container.CreateChild<CombinationLockWheel>();
                wheel.Init(p, values);
                return wheel;
            }).ToList();
        }

        public void Clear()
        {
            _wheels.ForEach(w => w.QueueFree());
            _wheels.Clear();
        }
    }
}