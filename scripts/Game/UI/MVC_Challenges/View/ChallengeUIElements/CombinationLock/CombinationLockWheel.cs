
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CombinationLockWheel : Control
    {
        public Action<string, string> OnValueSelected;

        public CombinationLockWheel(string paramName, IEnumerable<ChallengeValue> values)
        {
            
        }
    }
}