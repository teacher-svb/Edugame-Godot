namespace TnT.EduGame.Question
{
    // Null Object for a challenge without a visualization: rolls like the base class
    // and renders an empty widget, so nothing downstream needs a null check.
    // Not a GlobalClass: in the inspector, "no visualization" is simply leaving the field empty.
    public partial class NoVisualization : ChallengeVisualization
    {
        public static NoVisualization Instance { get; } = new();

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/None.tscn";
    }
}
