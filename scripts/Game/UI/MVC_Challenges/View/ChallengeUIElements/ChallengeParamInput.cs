
using System;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class ChallengeParamInput : Control
    {
        public Action<string, string> OnParamChanged;

        public void Clear()
        {
            this.GetChildren().ForEach(w => w.QueueFree());
        }
    }
}