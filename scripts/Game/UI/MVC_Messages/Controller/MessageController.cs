
using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.UI
{
    public partial class MessageController : Node { 
        public static MessageController Instance { get; private set; }
        // assigned in managers.tscn; no `= new()` defaults, those created nodes nobody freed
        [Export]
        public MessageView view;
        [Export]
        public MessageModel model;
        public int Count => model.messages.Count;

        public Action NextBtnPushed;
        public Action CloseBtnPushed;
        public override void _Ready()
        {
            Instance = this;
            view.InitializeView();
            view.NextBtnPushed += () => NextBtnPushed?.Invoke();
            view.CloseBtnPushed += () => CloseBtnPushed?.Invoke();
        }

        public async Task Show(float duration = .2f)
        {
            var nextMsg = model.messages.Dequeue();
            view.SetMessage(nextMsg.text, nextMsg.characterScenePath, nextMsg.name);
            await view.ShowView(duration);
        }

        public async Task Hide(float duration = .2f)
        {
            await view.HideView(duration);
        }

        public void AddMessage(string text, string characterScenePath, string name)
        {
            model.messages.Enqueue(new() { text = text, characterScenePath = characterScenePath, name = name });
        }

        internal void Clear()
        {
            model.messages.Clear();
        }
    }
}