using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.GameState;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Shows a line spoken by a character and waits until the player dismisses it,
    /// for reactions that aren't an objective's own start/complete/fail text.
    /// </summary>
    /// <remarks>
    /// Skipped when the reaction has no objective: that is the catch-up run of a
    /// <see cref="QuestsCompletedListener"/> in _Ready (quests completed earlier, e.g. a loaded
    /// save), which happens mid scene load, where a message breaks the loading screen. The line
    /// is meant for the moment itself, so it isn't repeated after loading either.
    /// </remarks>
    [GlobalClass]
    public partial class MessageStep : QuestStep
    {
        [Export(PropertyHint.MultilineText)] string _text;
        [Export] CharacterData _character;

        public override Task Run(QuestObjective objective)
        {
            if (objective == null)
                return Task.CompletedTask;

            var dismissed = new TaskCompletionSource();
            StateManagerGame.Instance.ShowMessage(_text, _character, () => dismissed.TrySetResult());
            return dismissed.Task;
        }
    }
}
