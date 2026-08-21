using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CombinationLockWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var container = this.CreateChild<HBoxContainer>();
            container.SetAnchorsPreset(LayoutPreset.FullRect);

            Challenge.FormulaParams.ForEach(p =>
            {
                var wheel = container.CreateChild<CombinationLockWheel>();
                var candidates = Challenge.Values.Where(v => v.ParamName == p);
                wheel.ValueSelected += (paramName, value) => EmitValueAssigned(paramName, value);
                wheel.Init(p, candidates);
            });
        }
    }
}
