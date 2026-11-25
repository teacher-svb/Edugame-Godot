using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.GameStates;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Shows a line spoken by a character and waits until the player dismisses it,
    /// for reactions that aren't an objective's own start/complete/fail text.
    /// </summary>
    /// <remarks>
    /// Skipped when the reaction has no objective: that is a catch-up run (a
    /// <see cref="QuestsCompletedListener"/> or a <c>_catchUpAfterLoad</c> listener, run once the scene is
    /// ready and bound, for quests completed earlier, e.g. in a loaded save). The line is meant for the
    /// moment itself, so it isn't repeated after loading (a reaction resumed with <c>_resumeAfterLoad</c>
    /// passes its objective, so it does show the line again).
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
