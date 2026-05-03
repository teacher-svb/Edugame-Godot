using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.EduGame.Characters;
using TnT.Input;
using TnT.Systems.UI;

namespace TnT.EduGame.GameState
{
    [GlobalClass]
    public partial class GameStateMessage : BaseGameState, IStateObject<GameStateMessage.MessageOptions>
    {
        public struct MessageOptions
        {
            public string text;
            public CharacterData character;
            public InputAction next;
            public InputAction close;
        }

        private bool retrievingNextMsg;
        private bool allMessagesRead = false;
        MessageOptions _options;

        public BaseState GetState(MessageOptions options)
        {
            _options = options;
            allMessagesRead = false;
            MessageController.Instance.AddMessage(options.text, options.character.CharacterFace, options.character.CharacterName);
            if (MessageController.Instance.Count > 1)
                return new BaseState(new() { ExitOnNextUpdate = () => true });

            return new BaseState(new() { OnEnter = Open, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            if (_options.close.Triggered)
            {
                ClearMessages();
            }
            if (_options.next.Triggered)
            {
                NextMessage();
            }
        }

        bool Exit()
        {
            return allMessagesRead;
        }

        void ClearMessages()
        {
            if (allMessagesRead == false)
            {
                MessageController.Instance.Clear();
                allMessagesRead = true;
            }
        }

        async void NextMessage()
        {
            GD.Print("next message");
            if (allMessagesRead == false && MessageController.Instance.Count == 0)
            {
                ClearMessages();
                return;
            }


            if (MessageController.Instance.Count > 0 && retrievingNextMsg == false)
            {
                retrievingNextMsg = true;
                await MessageController.Instance.Hide(.1f);
                await MessageController.Instance.Show();
                retrievingNextMsg = false;
            }
        }

        async Task Open()
        {
            MessageController.Instance.NextBtnPushed += NextMessage;
            MessageController.Instance.CloseBtnPushed += ClearMessages;
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await MessageController.Instance.Show();
            _options.next.Enable();
            _options.close.Enable();
        }

        async Task Close()
        {
            MessageController.Instance.NextBtnPushed -= NextMessage;
            MessageController.Instance.CloseBtnPushed -= ClearMessages;
            _options.next.Disable();
            _options.close.Disable();
            await MessageController.Instance.Hide();
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;
        }
    }
}