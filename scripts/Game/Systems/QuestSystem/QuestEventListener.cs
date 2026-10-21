using System;
using System.Linq;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Listens for quest objective updates, filters them by objective id and/or state, and
    /// on a match raises the base event and runs its child <see cref="QuestStep"/>s.
    /// </summary>
    /// <remarks>
    /// Observer + Command: the listener decides when, each step knows what and with which
    /// arguments. Steps run top to bottom in the scene tree (drag to reorder), each awaited
    /// before the next. Not done with signals: EmitSignal never awaits its handlers, and
    /// editor binds can't hold node references.
    /// </remarks>
    [GlobalClass]
    public partial class QuestEventListener : EventListener
    {
        [Export] bool _checkObjectiveId;
        [Export] string _objectiveId;
        [Export] bool _checkState;
        [Export] QuestState _state;

        // Wrap the whole sequence, not each step, e.g. to block input while it runs
        [Signal] public delegate void ReactionStartEventHandler();
        [Signal] public delegate void ReactionEndEventHandler();

        // Any objective that fails an enabled filter stops processing entirely (return, not continue)
        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective o)
                {
                    if (_checkObjectiveId && o.ObjectiveId != _objectiveId) return;
                    if (_checkState && o.State != _state) return;
                    base.Raise(o);
                    RunSteps(o);
                }
            }
        }

        // async void: fire-and-forget from Raise. A failing step is logged and skipped;
        // OperationCanceledException stops the remaining steps. finally guarantees ReactionEnd.
        // KNOWN ISSUE: no timeout. A step that never completes stalls the steps after it.
        private async void RunSteps(QuestObjective objective)
        {
            EmitSignal(SignalName.ReactionStart);
            try
            {
                foreach (var step in GetChildren().OfType<QuestStep>())
                {
                    try
                    {
                        await step.Run(objective);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr($"QuestEventListener '{Name}': step '{step.Name}' failed: {e.Message}");
                    }
                }
            }
            finally
            {
                EmitSignal(SignalName.ReactionEnd);
            }
        }
    }
}
