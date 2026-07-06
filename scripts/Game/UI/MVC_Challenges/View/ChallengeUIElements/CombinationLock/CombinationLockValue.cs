
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
}