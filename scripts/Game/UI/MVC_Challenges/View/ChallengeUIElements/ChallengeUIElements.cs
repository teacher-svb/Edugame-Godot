
using System;
using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeSelect : Control
    {
        public Action<int> OnValueSelected;
    }

    public partial class ChallengeParamInput : Control
    {
        public Action<string, int> OnParamChanged;
    }
    
    public partial class ChallengeValueView : Control
    {

    }
}