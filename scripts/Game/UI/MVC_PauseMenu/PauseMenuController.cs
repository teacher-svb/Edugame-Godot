using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class PauseMenuController : Node
    {
        public static PauseMenuController Instance { get; private set; }

        [Export] public PauseMenuView view;
        [Export] public PauseMenuModel model;

        public event Action OnResume;
        public event Action OnSettings;
        public event Action OnSave;
        public event Action OnLoad;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            view.OnResume   += () => OnResume?.Invoke();
            view.OnSettings += () => OnSettings?.Invoke();
            view.OnSave     += () => OnSave?.Invoke();
            view.OnLoad     += () => OnLoad?.Invoke();
            view.OnQuit     += () => CleanQuit.Quit(GetTree());   // no state involved: the game ends here
            QuestSystem.QuestStepListener.BlockingReactionsChanged += RefreshSaveEnabled;
        }

        public override void _ExitTree() => QuestSystem.QuestStepListener.BlockingReactionsChanged -= RefreshSaveEnabled;

        // "Laden" only with a save to load (refreshed on every Show and after a save)
        public void RefreshLoadEnabled() => view.SetLoadEnabled(SaveLoadManager.Instance?.HasSave() ?? false);

        // "Opslaan" not while a quest reaction runs (SaveLoadManager.CanSave); refreshed on Show and whenever one
        // starts or ends
        public void RefreshSaveEnabled() =>
            view.SetSaveEnabled(SaveLoadManager.Instance?.CanSave ?? false, "Opslaan kan even niet: er gebeurt nog iets in het spel.");

        public async Task Show()
        {
            RefreshLoadEnabled();
            RefreshSaveEnabled();
            await view.ShowView();
        }
        public async Task Hide() => await view.HideView();

        /// <summary>A screen opened on top of the menu (settings): no focus behind it.</summary>
        public void Pause() => view.ReleaseMenuFocus();

        /// <summary>Back from a screen on top: the focus on the first enabled button again.</summary>
        public void Resume() => view.FocusFirst();
    }
}
