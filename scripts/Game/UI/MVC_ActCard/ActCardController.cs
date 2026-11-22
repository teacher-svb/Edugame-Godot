using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    /// <summary>The card shown between acts (card act1-end-of-act), through <c>StateManagerGame.ShowActCard</c>.</summary>
    [GlobalClass]
    public partial class ActCardController : Node
    {
        public static ActCardController Instance { get; private set; }

        [Export] public ActCardView view;
        [Export] public ActCardModel model;

        public override void _EnterTree() => Instance = this;

        public async Task Show(string title, string subtitle)
        {
            model.Title = title ?? "";
            model.Subtitle = subtitle ?? "";
            await view.ShowView(model.Title, model.Subtitle);
        }

        public async Task ShowHint() => await view.ShowHint();
        public async Task Hide() => await view.HideView();
    }
}
