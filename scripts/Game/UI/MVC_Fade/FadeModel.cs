using Godot;


namespace TnT.EduGame.UI
{
    /// <summary>
    /// Empty: the fade has no state of its own. Kept so that every UI controller in <c>managers.tscn</c> has its view and
    /// model node (the same MVC layout as the others); removing it means editing that scene's node and the controller's
    /// <c>model</c> export together.
    /// </summary>
    [GlobalClass]
    public partial class FadeModel : Node
    {
    }
}