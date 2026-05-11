using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class PauseMenuController : Node
    {
        public static PauseMenuController Instance { get; private set; }

        [Export] public PauseMenuView view;
        [Export] public PauseMenuModel model;

        public event Action OnResume;
        public event Action OnSettings;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            view.OnResume += () => OnResume?.Invoke();
            view.OnSettings += () => OnSettings?.Invoke();
        }

        public async Task Show() => await view.ShowView();
        public async Task Hide() => await view.HideView();
    }
}
