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
                OnResume = () => Resume(session),
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
            // no character: a text-only box (no closeup, no name)
            MessageController.Instance.AddMessage(options.text, options.character?.CharacterScenePath, options.character?.CharacterName);
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
            try
            {
                await MessageController.Instance.Hide(.1f);
                // CleanQuit frees the autoloads a few frames before the engine quits
                if (!GodotObject.IsInstanceValid(MessageController.Instance))
                    return;
                // checked again: a close during the hide emptied the queue (Show would dequeue from an empty queue)
                if (session.AllRead)
                    return;
                if (MessageController.Instance.Count == 0)
                {
                    ClearMessages(session);
                    return;
                }
                await MessageController.Instance.Show();
            }
            finally
            {
                session.Retrieving = false;
            }
        }

        async Task Open(Session session)
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await MessageController.Instance.Show();
            // only once the box is shown: a Next during the scale-in started a Hide that killed the scale-in tween
            // (UITweens now resumes such an awaiter anyway)
            MessageController.Instance.NextBtnPushed += session.OnNext;
            MessageController.Instance.CloseBtnPushed += session.OnClose;
            session.Options.next.Enable();
            session.Options.close.Enable();
        }

        // a state on top of the box closed (the box is still showing): it may have unpaused the tree and disabled
        // the shared next/close actions
        Task Resume(Session session)
        {
            ManagerUI.Instance.GetTree().Paused = true;
            session.Options.next.Enable();
            session.Options.close.Enable();
            return Task.CompletedTask;
        }

        async Task Close(Session session)
        {
            // removed before it was read to the end (a scene load unwound it): its lines must not show in the next box
            ClearMessages(session);
            MessageController.Instance.NextBtnPushed -= session.OnNext;
            MessageController.Instance.CloseBtnPushed -= session.OnClose;
            session.Options.next.Disable();
            session.Options.close.Disable();
            await MessageController.Instance.Hide();
            ManagerUI.Instance.GetTree().Paused = false;
            if (_session == session)
                _session = null;
            // after the state is popped (AbstractStateStack removes a state before awaiting its ExitState), so a
            // message pushed by a callback (e.g. the next objective's start) opens a new box, and the ones after it
            // join that box
            foreach (var dismissed in session.Dismissed)
                dismissed();
        }
    }
}
