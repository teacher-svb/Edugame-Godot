using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class SettingsController : Node
    {
        public static SettingsController Instance { get; private set; }

        [Export] public SettingsView view;
        [Export] public SettingsModel model;

        public event Action OnBack;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            // view.OnBack += () => OnBack?.Invoke();
        }

        public async Task Show(bool audio, bool display, bool controls, bool accessibility)
        {
            // view.ShowPanels(audio, display, controls, accessibility);
            // await view.ShowView();
        }

        public async Task Hide() => await view.HideView();
    }
}
