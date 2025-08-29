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
    public partial class GameStateMessage : GameState, IStateObject<GameStateMessage.MessageOptions>
    {
        public struct MessageOptions
        {
            public string text;
            public CharacterData character;
        }
        [Export] public InputAction _next;
        [Export] public InputAction _close;

        // MessageController _uiController;
        private bool retrievingNextMsg;
        private bool allMessagesRead = false;

        // MessageController UIController
        // {
        //     get
        //     {
        //         if (_uiController == null)
        //             _uiController = UnityEngine.Object.FindAnyObjectByType<MessageController>();
        //         return _uiController;
        //     }
        // }

        public BaseState GetState(MessageOptions options)
        {
            allMessagesRead = false;
            MessageController.Instance.AddMessage(options.text, options.character.CharacterFace, options.character.CharacterName);
            // MessageController.Instance.AddMessage(options.text);
            if (MessageController.Instance.Count > 1)
                return new BaseState(new() { ExitOnNextUpdate = () => true });

            return new BaseState(new() { OnEnter = Open, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            if (_close.Triggered)
            {
                ClearMessages();
            }
            if (_next.Triggered)
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
            _next.Enable();
            _close.Enable();
        }

        async Task Close()
        {
            MessageController.Instance.NextBtnPushed -= NextMessage;
            MessageController.Instance.CloseBtnPushed -= ClearMessages;
            _next.Disable();
            _close.Disable();
            await MessageController.Instance.Hide();
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;
        }
    }
}