using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    // Removes a node from the level if it's still there (e.g. a picked-up item that the scene places again on load).
    // Safe to repeat, so it can run on a listener's catch-up after loading: a node that is already gone is skipped.
    [Tool, GlobalClass]
    public partial class FreeNodeStep : QuestStep
    {
        [Export] Node _node;

        public override Task Run(QuestObjective objective)
        {
            if (_node != null && IsInstanceValid(_node) && !_node.IsQueuedForDeletion())
                _node.QueueFree();
            return Task.CompletedTask;
        }
    }
}
