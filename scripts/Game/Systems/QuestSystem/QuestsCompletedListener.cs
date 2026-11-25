using System.Linq;
using Godot;
using Godot.Collections;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Runs its child <see cref="QuestStep"/>s once, as soon as every listed quest is completed,
    /// e.g. to open up a quest that follows a group of parallel quests (the arrows in the GDD
    /// quest diagram): wire ReactionEnd to whatever starts the next quest.
    /// </summary>
    /// <remarks>
    /// Also checks once the scene is ready and bound to the save data (<see cref="QuestStepListener.CallWhenSceneReady"/>:
    /// after every node's _Ready and the bindings, so a catch-up isn't overwritten by them), since quests completed in
    /// another scene or in a loaded save raised their events before this node existed. That catch-up run happens mid
    /// scene load and passes no objective, so its steps must not show anything (no quest messages, challenges;
    /// <see cref="MessageStep"/> skips itself): enable a trigger area instead of starting a quest directly. It
    /// emits ReactionStart/ReactionEnd (that is how areas get armed again) but not OnListen.
    /// Its reactions don't hold up saving: the catch-up runs them again after every load.
    /// </remarks>
    [GlobalClass]
    public partial class QuestsCompletedListener : QuestStepListener
    {
        [Export] Array<Quest> _quests = new();

        bool _triggered;

        public override void _Ready()
        {
            base._Ready();

            if (!_quests.Any(q => q != null))
            {
                GD.PrintErr($"QuestsCompletedListener '{Name}': No quests assigned.");
                return;
            }

            CallWhenSceneReady();
        }

        // the catch-up: quests completed before this scene was loaded
        protected override void OnSceneReady()
        {
            if (AllCompleted())
                Trigger(null);
        }

        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective { State: QuestState.COMPLETED } o && AllCompleted())
                    Trigger(o);
            }
        }

        void Trigger(QuestObjective objective)
        {
            if (_triggered)
                return;

            _triggered = true;
            if (objective != null)
                base.Raise(objective);   // OnListen only for a live completion, not with nothing on a catch-up
            RunSteps(objective);
        }

        // the registered quest is the one whose state the QuestManager tracks
        bool AllCompleted() => _quests
            .Where(q => q != null)
            .All(q => (QuestManager.Instance.Quests.FirstOrDefault(r => r.Id == q.Id) ?? q).IsCompleted);
    }
}
