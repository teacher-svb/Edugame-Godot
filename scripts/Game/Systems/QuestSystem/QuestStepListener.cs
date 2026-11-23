using System;
using System.Linq;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Base of the quest listeners: subclasses decide when to react, this runs the reaction,
    /// i.e. its child <see cref="QuestStep"/>s.
    /// </summary>
    /// <remarks>
    /// Observer + Command: the listener decides when, each step knows what and with which
    /// arguments. Steps run top to bottom in the scene tree (drag to reorder), each awaited
    /// before the next. Not done with signals: EmitSignal never awaits its handlers, and
    /// editor binds can't hold node references.
    /// </remarks>
    [GlobalClass]
    public abstract partial class QuestStepListener : EventListener
    {
        // Wrap the whole sequence, not each step, e.g. to block input while it runs
        [Signal] public delegate void ReactionStartEventHandler();
        [Signal] public delegate void ReactionEndEventHandler();

        // async void: fire-and-forget from Raise. A failing step is logged and skipped;
        // OperationCanceledException stops the remaining steps. finally guarantees ReactionEnd.
        // KNOWN ISSUE: no timeout. A step that never completes stalls the steps after it.
        // signals = false for a catch-up after loading that must not drive anything wired to them again
        protected async void RunSteps(QuestObjective objective, bool signals = true)
        {
            if (signals)
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
                        GD.PrintErr($"{GetType().Name} '{Name}': step '{step.Name}' failed: {e.Message}");
                    }
                }
            }
            finally
            {
                if (signals)
                    EmitSignal(SignalName.ReactionEnd);
            }
        }
    }
}
