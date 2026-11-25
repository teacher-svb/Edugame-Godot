using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using System.Collections.Generic;
using Godot;
using TnT.EduGame.Characters;
using TnT.Input;
using TnT.EduGame.UI;
namespace TnT.EduGame.GameStates
{
    /// <summary>
    /// Character dialogue. One message state shows a <see cref="Session"/>: the message it was opened with plus every
    /// message added while it is open (<see cref="Enqueue"/>), one after the other. Each message keeps its own
    /// <c>onDismissed</c> (for a quest message: the commit of its objective change); they all run, in order, once the
    /// box is closed and the state is popped, so a message pushed in response lands on a clean stack.
    /// </summary>
    [GlobalClass]
    public partial class GameStateMessage : BaseGameState, IStateObject<GameStateMessage.MessageOptions>
    {
        public struct MessageOptions
        {
            public string text;
            public CharacterData character;
            public InputAction next;
            public InputAction close;
            public Action onDismissed;
        }

        /// <summary>One open message box: its inputs, the callbacks of every message in it, and how far it is.</summary>
        sealed class Session
        {
            public MessageOptions Options;
            public readonly List<Action> Dismissed = new();
            public bool Opened;
            public bool AllRead;
            public bool Retrieving;
            public Action OnNext;
            public Action OnClose;
        }

        Session _session;

        /// <summary>True while a message box is pushed or showing: new messages go into it with <see cref="Enqueue"/>.</summary>
        public bool IsOpen => _session != null;

        public BaseState GetState(MessageOptions options)
        {
            var session = new Session { Options = options };
            session.OnNext = () => NextMessage(session);
            session.OnClose = () => ClearMessages(session);
            _session = session;
            Add(session, options);
            return new BaseState(new()
            {
                OnEnter = () => Open(session),
                OnExit = () => Close(session),
                ExitOnNextUpdate = () => session.AllRead,
                OnUpdate = () => Update(session),
            });
        }

        /// <summary>Adds a message to the open box; it shows after the ones before it.</summary>
        public void Enqueue(MessageOptions options)
        {
            if (_session == null)
                throw new InvalidOperationException("GameStateMessage.Enqueue: no message box open");
            Add(_session, options);
        }

        static void Add(Session session, MessageOptions options)
        {
            MessageController.Instance.AddMessage(options.text, options.character.CharacterScenePath, options.character.CharacterName);
            if (options.onDismissed != null)
                session.Dismissed.Add(options.onDismissed);
        }

        void Update(Session session)
        {
            if (session.Options.close.Triggered)
                ClearMessages(session);
            if (session.Options.next.Triggered)
                NextMessage(session);
        }

        static void ClearMessages(Session session)
        {
            if (session.AllRead)
                return;
            MessageController.Instance.Clear();
            session.AllRead = true;
        }

        static async void NextMessage(Session session)
        {
            if (session.AllRead || session.Retrieving)
                return;
            if (MessageController.Instance.Count == 0)
            {
                ClearMessages(session);
                return;
            }
            session.Retrieving = true;
            await MessageController.Instance.Hide(.1f);
            await MessageController.Instance.Show();
            session.Retrieving = false;
        }

        async Task Open(Session session)
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            // entered again after a state on top of it was popped: the box is already showing
            if (session.Opened)
                return;
            session.Opened = true;
            MessageController.Instance.NextBtnPushed += session.OnNext;
            MessageController.Instance.CloseBtnPushed += session.OnClose;
            await MessageController.Instance.Show();
            session.Options.next.Enable();
            session.Options.close.Enable();
        }

        async Task Close(Session session)
        {
            MessageController.Instance.NextBtnPushed -= session.OnNext;
            MessageController.Instance.CloseBtnPushed -= session.OnClose;
            session.Options.next.Disable();
            session.Options.close.Disable();
            await MessageController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
            if (_session == session)
                _session = null;
            // after the state is popped (AbstractStateStack.TransitionOut pops before awaiting ExitState), so a
            // message pushed by a callback (e.g. the next objective's start) opens a new box, and the ones after it
            // join that box
            foreach (var dismissed in session.Dismissed)
                dismissed();
        }
    }
}
