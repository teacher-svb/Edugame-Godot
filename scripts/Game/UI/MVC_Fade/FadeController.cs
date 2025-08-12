using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class FadeController : Control
    {
        [Export]
        public FadeView view = new();
        [Export]
        public FadeModel model = new();

        public override void _Ready()
        {
            Initialize();
        }

        async void Initialize() => await view.InitializeView(this);

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