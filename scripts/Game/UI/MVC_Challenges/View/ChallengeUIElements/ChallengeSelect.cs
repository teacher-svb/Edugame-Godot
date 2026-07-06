
using System;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class ChallengeValueSelect : Control
    {
        public Action<int> OnValueSelected;
    }
}