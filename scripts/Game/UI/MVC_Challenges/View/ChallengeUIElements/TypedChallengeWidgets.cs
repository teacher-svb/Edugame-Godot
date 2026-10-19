using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Typed variants of the slot widgets: `Settings` is the widget's own resource type,
    // and Init rejects a mismatched resource with a clear message instead of a cast error.
    // (Own file: Godot's generator allows one class name per file.)

    // Visualization widget with typed access to its resource's settings.
    public partial class ChallengeVisualizationWidget<TVisualization> : ChallengeVisualizationWidget
        where TVisualization : ChallengeVisualization
    {
        protected TVisualization Settings => (TVisualization)Element;

        protected override bool Accepts(ChallengeElement element) => element is TVisualization;
    }

    // Input widget with typed access to its resource's settings.
    public partial class ChallengeInputWidget<TInput> : ChallengeInputWidget
        where TInput : ChallengeInput
    {
        protected TInput Settings => (TInput)Element;

        protected override bool Accepts(ChallengeElement element) => element is TInput;
    }
}
