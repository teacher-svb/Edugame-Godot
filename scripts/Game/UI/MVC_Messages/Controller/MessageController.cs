
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TnT.Systems.UI
{
    public class MessageController : MonoBehaviour { 
        [SerializeField]
        public MessageView view = new();
        [SerializeField]
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
            await view.InitializeView();
        }

        public async Task Show()
        {
            var nextMsg = model.messages.Dequeue();
            view.SetMessage(nextMsg.text, nextMsg.sprite, nextMsg.name);
            view.Show();
            await Task.Yield();
        }

        public async Task Hide()
        {
            view.Hide();
            await Task.Yield();
        }

        public void AddMessage(string text, Sprite sprite, string name)
        {
            model.messages.Enqueue(new() { text = text, sprite = sprite, name = name });
        }

        internal void Clear()
        {
            model.messages.Clear();
        }
    }
}