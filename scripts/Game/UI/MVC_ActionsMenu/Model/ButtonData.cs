
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ButtonData : Resource
    {
        [Signal]
        public delegate void OnButtonClickedEventHandler();
        [Export] public string buttonLabel = "";
        // public OnButtonClickedEventHandler OnButtonClicked;
    }
}
