using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Autosave: saves the game (<see cref="SaveLoadManager.SaveGame()"/>), e.g. when an act ends. Skipped on a
    /// listener's catch-up run (no objective): nothing new happened, the state came from the save or another scene.
    /// </summary>
    /// <remarks>
    /// Saving is refused while a quest reaction runs (finding 5.1). This step's own reaction doesn't count (it is the
    /// one asking): under a <see cref="QuestsCompletedListener"/> (end of an act) it never counts anyway, because a
    /// load replays those reactions. If another reaction is running, the step waits until it has ended, then saves.
    /// Under a <see cref="QuestEventListener"/> the save stores its objective mid-reaction: set its Resume After
    /// Load, or a load leaves that objective stuck.
    /// </remarks>
    [GlobalClass]
    public partial class SaveGameStep : QuestStep
    {
        public override async Task Run(QuestObjective objective)
        {
            if (objective == null || SaveLoadManager.Instance == null)
                return;

            var listener = GetParent() as QuestStepListener;
            if (listener is QuestEventListener { ResumesAfterLoad: false })
                GD.PushWarning($"SaveGameStep '{GetPath()}': autosave inside an objective's reaction without Resume After Load; a load can't finish that objective");

            // another reaction running: wait for it (the tree may be paused: process_frame still comes)
            while (QuestStepListener.BlockingReactionsExcept(listener) > 0)
            {
                if (!IsInstanceValid(this) || !IsInsideTree())
                    return;   // the level was left meanwhile: nothing to save from here
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (IsInstanceValid(this) && IsInsideTree())
                SaveLoadManager.Instance?.SaveGame(listener);
        }
    }
}
