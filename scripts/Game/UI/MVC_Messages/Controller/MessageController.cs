
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
            Characters.Character3D.TurnSpeakerToPlayer(nextMsg.character);
            await view.ShowView(duration);
        }

        public async Task Hide(float duration = .2f)
        {
            await view.HideView(duration);
        }

        /// <param name="character">Who speaks the line (null: a text-only box): its closeup and name, and it turns
        /// toward the player when the line shows.</param>
        public void AddMessage(string text, Characters.CharacterData character)
        {
            model.messages.Enqueue(new() { text = text, characterScenePath = character?.CharacterScenePath, name = character?.CharacterName, character = character });
        }

        internal void Clear()
        {
            model.messages.Clear();
        }
    }
}