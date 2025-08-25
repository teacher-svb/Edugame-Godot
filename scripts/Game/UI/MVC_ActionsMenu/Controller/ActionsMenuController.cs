
using Godot;

namespace TnT.Systems.UI
{
    public partial class ActionsMenuController : Control { 
        [Export]
        public ActionsMenuView view = new ActionsMenuView();
        [Export]
        public ActionsMenuModel model = new ActionsMenuModel();
        void Start()
        {
            Initialize();
        }

        async void Initialize()
        {
            await view.InitializeView(this);

            foreach (var buttonData in model.Buttons)
            {
                view.AddButton(buttonData.buttonLabel, buttonData.OnButtonClicked.Invoke);
            }
        }
    }
}