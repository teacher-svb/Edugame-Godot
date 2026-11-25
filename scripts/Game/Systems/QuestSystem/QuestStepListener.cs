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
        // Wrap the whole sequence, not each step, e.g. to block input while it runs.
        // ReactionEnd only when every step ran to the end; ReactionCancelled when a step stopped the reaction
        // (OperationCanceledException, e.g. a failed challenge) or threw. Wire "complete the objective" to ReactionEnd.
        [Signal] public delegate void ReactionStartEventHandler();
        [Signal] public delegate void ReactionEndEventHandler();
        [Signal] public delegate void ReactionCancelledEventHandler();

        #region Saving while a reaction runs
        // Reactions that hold up saving (see RunSteps' blocksSaving), over all listeners. A save in the middle of
        // such a reaction stores its objective in a state whose reaction a load doesn't run again (finding 5.1).
        static int s_blockingReactions;
        int _blockingReactions;   // this listener's share of s_blockingReactions
        int _blockingEpoch;       // bumped when _ExitTree releases them: older runs then have nothing to release

        /// <summary>Raised when a reaction that holds up saving starts or ends (e.g. to refresh the Save button).</summary>
        public static event Action BlockingReactionsChanged;

        /// <summary>True while a reaction that holds up saving runs (in any listener).</summary>
        public static bool AnyBlockingReaction => s_blockingReactions > 0;

        /// <summary>The number of reactions holding up saving, not counting those of <paramref name="listener"/>.</summary>
        public static int BlockingReactionsExcept(QuestStepListener listener) =>
            s_blockingReactions - (listener?._blockingReactions ?? 0);

        void ChangeBlocking(int delta)
        {
            _blockingReactions += delta;
            s_blockingReactions += delta;
            BlockingReactionsChanged?.Invoke();
        }
        #endregion

        public override void _ExitTree()
        {
            base._ExitTree();
            StopWaitingForSceneReady();
            // a reaction interrupted by a scene change may never reach its finally (a step awaiting a freed node):
            // release its hold on saving here, its finally then finds nothing to release
            _blockingEpoch++;
            if (_blockingReactions != 0)
                ChangeBlocking(-_blockingReactions);
        }

        // async void: fire-and-forget from Raise. A step throwing OperationCanceledException (e.g. a failed
        // challenge) stops the remaining steps quietly; any other exception is logged and stops them too.
        // Either way the reaction is cancelled: ReactionCancelled instead of ReactionEnd.
        // KNOWN ISSUE: no timeout. A step that never completes stalls the steps after it.
        // signals = false for a catch-up after loading that must not drive anything wired to them again.
        // blocksSaving: SaveLoadManager refuses to save while it runs (reactions a load doesn't replay).
        protected async void RunSteps(QuestObjective objective, bool signals = true, bool blocksSaving = false)
        {
            // counted only while this listener is in the tree: _ExitTree releases what is still held
            bool counted = blocksSaving && IsInsideTree();
            int epoch = _blockingEpoch;
            if (counted)
                ChangeBlocking(1);
            bool completed = false;
            try
            {
                if (signals)
                    EmitSignal(SignalName.ReactionStart);
                foreach (var step in GetChildren().OfType<QuestStep>())
                {
                    await step.Run(objective);
                    // the level was left meanwhile (a step resumed after a scene change, e.g. a message box the
                    // loader closed): stop quietly, without ReactionEnd / ReactionCancelled (finding 5.5)
                    if (Interrupted)
                        return;
                }
                completed = true;
            }
            catch (OperationCanceledException)
            {
                // a step stopped the reaction on purpose (e.g. a failed challenge restarts its own objective)
            }
            catch (Exception e)
            {
                // a step that failed because the level was left under it (a freed node) is an interruption too
                if (Interrupted)
                    return;
                GD.PushError($"{GetType().Name} '{Name}': a step failed, the reaction is cancelled: {e}");
            }
            finally
            {
                if (counted && epoch == _blockingEpoch)
                    ChangeBlocking(-1);
            }

            if (signals && !Interrupted)
                EmitSignal(completed ? SignalName.ReactionEnd : SignalName.ReactionCancelled);
        }

        // freed or out of the tree (scene change): the rest of a reaction must not run
        bool Interrupted => !IsInstanceValid(this) || !IsInsideTree();

        #region After a scene load
        // Catch-ups and resumes after loading run once the scene is ready and bound to the save data (characters,
        // doors, challenges, quests; QuestManager.Bound), not in _Ready: later siblings' _Ready and the bindings
        // would overwrite them (finding 5.4). Bound comes once per scene loaded through the scene loader, after
        // every node's _Ready; a listener added after that runs it deferred.
        bool _awaitingSceneReady;

        /// <summary>Calls <see cref="OnSceneReady"/> once, when the scene this listener is in is ready and bound.</summary>
        protected void CallWhenSceneReady()
        {
            var manager = QuestManager.Instance;
            if (manager == null)
                return;
            if (manager.IsBoundTo(GetTree().CurrentScene))
            {
                CallDeferred(MethodName.RunSceneReady);
                return;
            }
            _awaitingSceneReady = true;
            manager.Bound += RunSceneReady;
        }

        void RunSceneReady()
        {
            StopWaitingForSceneReady();
            if (IsInsideTree())
                OnSceneReady();
        }

        void StopWaitingForSceneReady()
        {
            if (!_awaitingSceneReady)
                return;
            _awaitingSceneReady = false;
            if (QuestManager.Instance != null)
                QuestManager.Instance.Bound -= RunSceneReady;
        }

        /// <summary>The scene is ready and bound (see <see cref="CallWhenSceneReady"/>).</summary>
        protected virtual void OnSceneReady() { }
        #endregion
    }
}
