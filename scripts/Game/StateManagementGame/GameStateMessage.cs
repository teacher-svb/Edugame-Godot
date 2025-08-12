using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.GameState
{
    [Serializable]
    public class GameStateMessage : IStateObject<GameStateMessage.MessageOptions>, IGameState
    {
        public struct MessageOptions
        {
            public string text;
            public CharacterData character;
        }
        // [SerializeField] InputActionReference next;
        // [SerializeField] InputActionReference close;

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
            // UIController.AddMessage(options.text, options.character.CharacterFace, options.character.CharacterName);
            // if (UIController.Count > 1)
            //     return new BaseState(new() { ExitOnNextUpdate = () => true });

            return new BaseState(new() { OnEnter = Open, OnExit = Close, ExitOnNextUpdate = Exit, OnUpdate = Update });
        }

        private void Update()
        {
            // if (close.action.triggered)
            // {
            //     ClearMessages();
            // }
            // if (next.action.triggered)
            // {
            //     NextMessage();
            // }
        }

        bool Exit()
        {
            return allMessagesRead;
        }

        void ClearMessages()
        {
            // if (allMessagesRead == false)
            // {
            //     UIController.Clear();
            //     allMessagesRead = true;
            // }
        }

        async void NextMessage()
        {
            // if (allMessagesRead == false && UIController.Count == 0)
            // {
            //     ClearMessages();
            //     return;
            // }


            // if (UIController.Count > 0 && retrievingNextMsg == false)
            // {
            //     retrievingNextMsg = true;
            //     await UIController.Hide();
                await Task.Yield();
            //     await UIController.Show();
            //     retrievingNextMsg = false;
            // }
        }

        async Task Open()
        {
            // UIController.NextBtnPushed += NextMessage;
            // UIController.CloseBtnPushed += ClearMessages;
            // ETime[play].timeScale = 0;
            // await UIController.Show();
            // next.action.Enable();
            // close.action.Enable();
        }

        async Task Close()
        {
            // UIController.NextBtnPushed -= NextMessage;
            // UIController.CloseBtnPushed -= ClearMessages;
            // next.action.Disable();
            // close.action.Disable();
            // await UIController.Hide();
            // ETime[play].timeScale = 1;
        }
    }
}