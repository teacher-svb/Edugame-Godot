using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// One step of a quest reaction: a Command that knows its receiver and arguments, so the
    /// quest only has to say "go". Add steps as children of a <see cref="QuestEventListener"/>;
    /// they run top to bottom, each awaited before the next starts.
    /// </summary>
    /// <remarks>
    /// Run() completes when the step is done. Throw OperationCanceledException to stop the
    /// remaining steps quietly (e.g. a failed challenge that restarts its own objective).
    /// Don't mark a step [Tool] when it exports a non-tool script type (Character3D,
    /// MathChallengeTrigger, ...): in the editor those nodes are plain Node placeholders and
    /// assigning them to the typed export throws InvalidCastException. CallMethodStep is
    /// [Tool] for its method dropdown, which is safe because its target is a plain Node.
    /// </remarks>
    [Tool]
    public abstract partial class QuestStep : Node
    {
        public abstract Task Run(QuestObjective objective);
    }
}
