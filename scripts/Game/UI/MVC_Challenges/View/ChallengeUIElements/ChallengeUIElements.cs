
using System;
using Godot;

namespace TnT.Systems.UI
{
    [Serializable]
    public partial class ChallengeSelect : Control
    {
        public Action<int> OnValueSelected;
    }

    [Serializable]
    public partial class ChallengeParamInput : Control
    {
        public Action<string, int> OnParamChanged;
    }
    
    [Serializable]
    public partial class ChallengeValueView : Control
    {

    }
}