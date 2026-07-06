using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class StartMenuModel : Node
    {
        public bool HasSave() => SaveLoadManager.Instance.HasSave();
    }
}
