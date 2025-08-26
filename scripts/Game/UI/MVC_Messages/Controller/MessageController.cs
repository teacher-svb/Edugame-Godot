
using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    public partial class MessageController : Control { 
        [Export]
        public MessageView view = new();
        [Export]
        public MessageModel model = new();
        public int Count => model.messages.Count;

        public Action NextBtnPushed;
        public Action CloseBtnPushed;
        void Start()
        {
            Initialize();
            view.NextBtnPushed += () => NextBtnPushed();
            view.CloseBtnPushed += () => CloseBtnPushed();
        }

        async void Initialize()
        {
            await view.InitializeView(this);
        }

        public async Task ShowView()
        {
            var nextMsg = model.messages.Dequeue();
            view.SetMessage(nextMsg.text, nextMsg.sprite, nextMsg.name);
            view.Show();
            await Task.Yield();
        }

        public async Task HideView()
        {
            view.Hide();
            await Task.Yield();
        }

        public void AddMessage(string text, Texture2D sprite, string name)
        {
            model.messages.Enqueue(new() { text = text, sprite = sprite, name = name });
        }

        internal void Clear()
        {
            model.messages.Clear();
        }
    }
}