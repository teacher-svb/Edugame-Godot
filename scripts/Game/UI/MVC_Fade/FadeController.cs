using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class FadeController : Node
    {
        [Export] public FadeView view;
        [Export] public FadeModel model;

        public override void _Ready() => view.InitializeView();

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