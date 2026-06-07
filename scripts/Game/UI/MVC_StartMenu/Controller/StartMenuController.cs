using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class StartMenuController : Node
    {
        public static StartMenuController Instance { get; private set; }

        [Export] public StartMenuView view;
        [Export] public StartMenuModel model;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            view.SetLoadEnabled(model.HasSave());
            view.OnNewGame += () => SaveLoadManager.Instance.NewGame();
            view.OnLoadGame += () => SaveLoadManager.Instance.LoadGame(SaveLoadManager.Instance.GameData.Name);
        }
    }
}
