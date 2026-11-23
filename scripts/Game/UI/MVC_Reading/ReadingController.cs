using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Reading;

namespace TnT.Systems.UI
{
    /// <summary>The reading panel for longer texts (R2), through <c>StateManagerGame.ShowReading</c>.</summary>
    [GlobalClass]
    public partial class ReadingController : Node
    {
        public static ReadingController Instance { get; private set; }

        [Export] public ReadingView view;
        [Export] public ReadingModel model;

        public override void _EnterTree() => Instance = this;

        public bool IsLastPage => model.IsLastPage;
        public bool ReachedEnd => model.ReachedEnd;

        public async Task Show(ReadableText text)
        {
            model.SetText(text);
            view.ShowPage(model.CurrentPage, model.PageIndex, model.PageCount, model.IsLastPage);
            await view.ShowView(model.Title, model.Look);
        }

        /// <summary>Turns to the next page; false when already on the last one.</summary>
        public async Task<bool> NextPage()
        {
            if (!model.NextPage())
                return false;
            await view.TurnPage(model.CurrentPage, model.PageIndex, model.PageCount, model.IsLastPage);
            return true;
        }

        public async Task Hide() => await view.HideView();
    }
}
