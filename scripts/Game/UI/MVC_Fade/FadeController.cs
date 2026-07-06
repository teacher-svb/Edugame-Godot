using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class FadeController : Node
    {
        [Export] public FadeView view;
        [Export] public FadeModel model;

        public override void _Ready()
        {
            Initialize();
        }

        async void Initialize() => await view.InitializeView();

        public async Task ShowView()
        {
            await view.Show();
        }

        public async Task HideView()
        {
            await view.Hide();
        }
    }
}