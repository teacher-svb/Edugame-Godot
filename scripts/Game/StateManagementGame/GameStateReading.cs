using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Reading;
using TnT.Input;
using TnT.Systems.State;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    /// <summary>
    /// Reading a longer text (letter, notice): pauses the game and shows the reading panel. Next turns the page, and
    /// on the last page closes it; close closes it on any page. <c>onClosed(readToEnd)</c> runs once the state is
    /// popped, with whether the last page was reached.
    /// </summary>
    [GlobalClass]
    public partial class GameStateReading : BaseGameState, IStateObject<GameStateReading.ReadingOptions>
    {
        public struct ReadingOptions
        {
            public ReadableText text;
            public InputAction next;
            public InputAction close;
            public Action<bool> onClosed;
        }

        ReadingOptions _options;
        bool _ready;
        bool _turning;
        bool _done;

        public BaseState GetState(ReadingOptions options)
        {
            _options = options;
            _ready = false;
            _turning = false;
            _done = false;
            return new BaseState(new() { OnEnter = Open, OnExit = Close, ExitOnNextUpdate = () => _done, OnUpdate = Update });
        }

        void Update()
        {
            if (!_ready)
                return;
            if (_options.close.Triggered)
                Dismiss();
            else if (_options.next.Triggered)
                Next();
        }

        async Task Open()
        {
            ManagerUI.Instance.GetTree().Paused = true;
            await ReadingController.Instance.Show(_options.text);
            _options.next.Enable();
            _options.close.Enable();
            _ready = true;
        }

        async Task Close()
        {
            _options.next.Disable();
            _options.close.Disable();
            bool readToEnd = ReadingController.Instance.ReachedEnd;
            await ReadingController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
            _options.onClosed?.Invoke(readToEnd);
        }

        /// <summary>As if the player pressed next: the next page, or close on the last one. Also for tests.</summary>
        public async void Next()
        {
            if (!_ready || _turning || _done)
                return;
            if (ReadingController.Instance.IsLastPage)
            {
                _done = true;
                return;
            }
            _turning = true;
            await ReadingController.Instance.NextPage();
            _turning = false;
        }

        /// <summary>As if the player pressed close: closes on any page. Also for tests.</summary>
        public void Dismiss()
        {
            if (_ready)
                _done = true;
        }
    }
}
