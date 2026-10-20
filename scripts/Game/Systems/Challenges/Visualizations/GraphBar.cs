using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // One bar of a BarGraphVisualization: a category label and the param that sets its height.
    [GlobalClass, Tool]
    public partial class GraphBar : ParamAwareResource
    {
        [Export] public string Label { get; set; } = "";
        [Export] public string Param { get; set; } = "";

        public GraphBar() { }
        public GraphBar(string label, string param)
        {
            Label = label;
            Param = param;
        }

        protected override IEnumerable<string> ParamFields => [nameof(Param)];
    }
}
