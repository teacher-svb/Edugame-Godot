
using System;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class ChallengeSelect : Control
    {
        public Action<int> OnValueSelected;
    }
}