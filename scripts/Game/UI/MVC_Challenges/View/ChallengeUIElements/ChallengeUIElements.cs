
using System;
using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeSelect : OptionButton
    {
        public Action<int> OnValueSelected;
    }

    public partial class ChallengeParamInput : TextEdit
    {
        public Action<string, string> OnParamChanged;
    }
    
    public partial class ChallengeValueView : Label
    {

    }
}