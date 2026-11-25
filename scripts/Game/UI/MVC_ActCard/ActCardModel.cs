using Godot;

namespace TnT.EduGame.UI
{
    /// <summary>What the act card shows: the act's title and a subtitle.</summary>
    [GlobalClass]
    public partial class ActCardModel : Node
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
    }
}
