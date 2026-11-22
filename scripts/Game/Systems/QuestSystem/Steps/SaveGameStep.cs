using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Autosave: saves the game (<see cref="SaveLoadManager.SaveGame"/>), e.g. when an act ends. Skipped on a
    /// listener's catch-up run (no objective): nothing new happened, the state came from the save or another scene.
    /// </summary>
    [GlobalClass]
    public partial class SaveGameStep : QuestStep
    {
        public override Task Run(QuestObjective objective)
        {
            if (objective != null)
                SaveLoadManager.Instance?.SaveGame();
            return Task.CompletedTask;
        }
    }
}
