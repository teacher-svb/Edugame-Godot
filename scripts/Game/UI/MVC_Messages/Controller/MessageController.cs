
using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    public partial class MessageController : Node { 
        public static MessageController Instance { get; private set; }
        [Export]
        public MessageView view = new();
        [Export]
        public MessageModel model = new();
        public int Count => model.messages.Count;

        public Action NextBtnPushed;
        public Action CloseBtnPushed;
        public override async void _Ready()
        {
            Instance = this;
            Initialize();
            view.NextBtnPushed += () => NextBtnPushed?.Invoke();
            view.CloseBtnPushed += () => CloseBtnPushed?.Invoke();
        }

        async void Initialize()
        {
            await view.InitializeView();
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

        public void AddMessage(string text)
        {
            model.messages.Enqueue(new() { text = text });
        }

        internal void Clear()
        {
            model.messages.Clear();
        }
    }
}