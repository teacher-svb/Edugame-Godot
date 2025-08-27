
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
        public override void _Ready()
        {
            Initialize();
            view.NextBtnPushed += () => NextBtnPushed();
            view.CloseBtnPushed += () => CloseBtnPushed();
            Instance = this;
        }

        async void Initialize()
        {
            await view.InitializeView();
        }

        public async Task Show()
        {
            var nextMsg = model.messages.Dequeue();
            view.SetMessage(nextMsg.text, nextMsg.sprite, nextMsg.name);
            await view.ShowView();
        }

        public async Task Hide()
        {
            await view.HideView();
        }

        public void AddMessage(string text, Texture2D sprite, string name)
        {
            model.messages.Enqueue(new() { text = text, sprite = sprite, name = name });
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