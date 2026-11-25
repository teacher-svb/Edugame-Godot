using Godot;
using TnT.Systems;

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
            view.OnNewGame += () => SaveLoadManager.Instance.NewGame();
            view.OnLoadGame += () => SaveLoadManager.Instance.LoadGame(model.SaveName);
            view.OnSettings += OpenSettings;
            view.OnQuit += () => CleanQuit.Quit(GetTree());
            // every time the menu is shown again (back from settings), not only at setup: the save may have
            // appeared or gone in the meantime
            view.VisibilityChanged += OnViewVisibilityChanged;
            Refresh();
        }

        public override void _ExitTree()
        {
            if (IsInstanceValid(view))
                view.VisibilityChanged -= OnViewVisibilityChanged;
        }

        void OnViewVisibilityChanged()
        {
            if (view.IsVisibleInTree())
                Refresh();
        }

        // Load enabled only with a save, then the focus (keyboard/controller: Space/Enter presses, arrows move) on
        // Load or New Game; a disabled Load can't keep the focus
        void Refresh()
        {
            view.SetLoadEnabled(model.HasSave());
            view.FocusFirst();
        }

        // The settings screen sits on the managers' UI layer, below this scene's menu: the menu hides while it is
        // open and comes back when it closes. Only from the idle start menu (not while a game is being loaded).
        void OpenSettings()
        {
            var states = GameStates.StateManagerGame.Instance;
            if (states == null || !states.IsIdle)
                return;
            _ = view.HideView();
            states.OpenSettings(audio: true, display: true, controls: false, accessibility: true, onClosed: () =>
            {
                if (IsInstanceValid(view) && view.IsInsideTree())
                    _ = view.ShowView();
            });
        }
    }
}
