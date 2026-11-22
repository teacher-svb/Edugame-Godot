using System.Threading.Tasks;
using Godot;
using TnT.EduGame.GameState;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Shows the card between acts (<see cref="StateManagerGame.ShowActCard"/>) and waits until it is closed. Like
    /// <see cref="MessageStep"/>, skipped on a listener's catch-up run (no objective: a scene load or a loaded save),
    /// so the card shows once, when the act ends.
    /// </summary>
    [GlobalClass]
    public partial class ActCardStep : QuestStep
    {
        [Export] string _title = "";
        [Export] string _subtitle = "";

        public override Task Run(QuestObjective objective)
        {
            if (objective == null)
                return Task.CompletedTask;
            var closed = new TaskCompletionSource();
            StateManagerGame.Instance.ShowActCard(_title, _subtitle, () => closed.TrySetResult());
            return closed.Task;
        }
    }
}
