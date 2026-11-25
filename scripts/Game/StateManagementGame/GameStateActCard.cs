using System;
using System.Threading.Tasks;
using Godot;
using TnT.Input;
using TnT.Systems.State;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
{
    /// <summary>
    /// The card between acts: pauses the game, shows the card and waits until the player presses next or close
    /// (not before the card is fully shown and <see cref="MinSeconds"/> have passed), then fades out and calls
    /// <c>onDismissed</c> once the state is popped.
    /// </summary>
    [GlobalClass]
    public partial class GameStateActCard : BaseGameState, IStateObject<GameStateActCard.ActCardOptions>
    {
        public struct ActCardOptions
        {
            public string title;
            public string subtitle;
            public InputAction next;
            public InputAction close;
            public Action onDismissed;
        }

        [Export] public double MinSeconds { get; set; } = 1.5;

        ActCardOptions _options;
        bool _ready;
        bool _done;

        public BaseState GetState(ActCardOptions options)
        {
            _options = options;
            _ready = false;
            _done = false;
            return new BaseState(new() { OnEnter = Open, OnResume = Resume, OnExit = Close, ExitOnNextUpdate = () => _done, OnUpdate = Update });
        }

        void Update()
        {
            if (_ready && (_options.next.Triggered || _options.close.Triggered))
                _done = true;
        }

        async Task Open()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await ActCardController.Instance.Show(_options.title, _options.subtitle);
            await ToSignal(tree.CreateTimer(MinSeconds, processAlways: true), SceneTreeTimer.SignalName.Timeout);
            _options.next.Enable();
            _options.close.Enable();
            _ready = true;
            await ActCardController.Instance.ShowHint();
        }

        // a state on top of it closed: it may have unpaused the tree and disabled the shared next/close actions
        Task Resume()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            if (_ready)
            {
                _options.next.Enable();
                _options.close.Enable();
            }
            return Task.CompletedTask;
        }

        async Task Close()
        {
            _options.next.Disable();
            _options.close.Disable();
            await ActCardController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
            _options.onDismissed?.Invoke();
        }

        /// <summary>For tests and skipping: closes the card as if the player pressed next (once it is shown).</summary>
        public void Dismiss()
        {
            if (_ready)
                _done = true;
        }
    }
}
