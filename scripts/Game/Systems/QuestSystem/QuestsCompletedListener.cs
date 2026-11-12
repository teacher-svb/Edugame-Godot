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
    /// Also checks on _Ready, since quests completed in another scene raised their events before
    /// this node existed. That run happens mid scene load, so its steps must not show anything
    /// (no quest messages, challenges): enable a trigger area instead of starting a quest directly.
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
                GD.PrintErr($"QuestsCompletedListener '{Name}': No quests assigned.");
            else if (AllCompleted())
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
            base.Raise(objective);
            RunSteps(objective);
        }

        // the registered quest is the one whose state the QuestManager tracks
        bool AllCompleted() => _quests
            .Where(q => q != null)
            .All(q => (QuestManager.Instance.Quests.FirstOrDefault(r => r.Id == q.Id) ?? q).IsCompleted);
    }
}
