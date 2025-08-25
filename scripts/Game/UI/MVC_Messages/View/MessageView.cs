using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TnT.Extensions;
using TnT.UI.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace TnT.Systems.UI
{
    [Serializable]
    public class MessageView
    {
        VisualElement root;
        VisualElement container;
        [SerializeField]
        UIDocument document;
        [SerializeField]
        StyleSheet styleSheet;

        Notification _message;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public async Task InitializeView()
        {
            root = document.rootVisualElement;
            root.Clear();

            root.styleSheets.Add(styleSheet);

            container = root.CreateChild("container");

            _message = container.CreateChild<Notification>("dialog");
            _message.Text = "lorem ipsum";

            _message.NextBtnPushed += () => NextBtnPushed();
            _message.CloseBtnPushed += () => CloseBtnPushed();

            await Task.Yield();
        }

        public void Show()
        {
            container.RemoveFromClassList("warning");
            container.RemoveFromClassList("error");
            container.AddToClassList("opened");
        }

        public void ShowWarning()
        {
            Show();
            container.AddToClassList("warning");
        }

        public void ShowError()
        {
            Show();
            container.AddToClassList("error");
        }

        public void Hide()
        {
            container.RemoveFromClassList("opened");
        }

        public void SetMessage(string text, Sprite sprite, string charName)
        {
            _message.Text = text;
            _message.Sprite = sprite;
            _message.Name = charName;
        }
    }
    public class Notification : VisualElement {
        Button _closeBtn;
        Button _nxtBtn;
        Image _image;

        Label _message;
        Label _name;
        public Action NextBtnPushed;
        public Action CloseBtnPushed;

        public string Text { get => _message.text; set => _message.text = value; }
        public string Name { get => _name.text; set => _name.text = value; }
        public Sprite Sprite { set => _image.sprite = value; }

        public Notification()
        {
            var textContainer = this.CreateChild("textContainer");
            _message = textContainer.CreateChild<Label>("text");
            _message.text = "Lorem ipsum dolor sid amed.";

            var characterContainer = this.CreateChild("character");
            _image = characterContainer.CreateChild<Image>("character_sprite");
            _name = characterContainer.CreateChild<Label>("character_name");

            var btnContainer = this.CreateChild("btnContainer");
            _closeBtn = btnContainer.CreateChild<Button>("closeBtn");
            _closeBtn.text = "<color=#A52A2A>󰢃</color> <color=#fff8e366>[󱊷]</color>";
            _nxtBtn = btnContainer.CreateChild<Button>("nextBtn");
            _nxtBtn.text = "<color=white>󰙢</color> <color=#fff8e366>[󱁐]</color>";

            _closeBtn.clicked += () => CloseBtnPushed();
            _nxtBtn.clicked += () => NextBtnPushed();
        }
    }

}
