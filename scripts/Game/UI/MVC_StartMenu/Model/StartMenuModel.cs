using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class StartMenuModel : Node
    {
        /// <summary>The game's own save, not the running game's (a dev checkpoint jump runs under its own save name).</summary>
        public string SaveName => SaveLoadManager.Instance.MainGameName;

        public bool HasSave() => SaveLoadManager.Instance.HasSave(SaveName);
    }
}
