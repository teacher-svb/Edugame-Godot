// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Sirenix.Utilities;
// using TnT.EduGame.Characters;
// using TnT.Extensions;
// using TnT.UI.Toolkit;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.TextCore.Text;
// using UnityEngine.UIElements;
// using static TnT.Systems.UI.CarouselCard;

// namespace TnT.Systems.UI
// {
//     [Serializable]
//     public class CharacterSelectionView
//     {
//         VisualElement root;
//         Carousel carousel;
//         [SerializeField]
//         UIDocument document;
//         [SerializeField]
//         StyleSheet styleSheet;
//         public delegate void OnSelectCharacter(CharacterData characterData);


//         public async Task InitializeView()
//         {
//             root = document.rootVisualElement;
//             root.Clear();

//             root.styleSheets.Add(styleSheet);

//             carousel = root.CreateChild<Carousel>("carousel");

//             await Task.Yield();
//         }

//         public void AddCharacter(CharacterData characterData, UnityEvent<CharacterData> onSelectCharacter)
//         {
//             carousel.AddContent(characterData.CharacterName, characterData.CharacterFace, () => { onSelectCharacter.Invoke(characterData); });
//         }
//     }

//     public partial class CarouselCard : VisualElement
//     {
//         public delegate void OnSelect();

//         public OnSelect OnSelectCard { get; private set; }

//         public void SetContent(string title, OnSelect onSelect)
//         {
//             var item = this.CreateChild<Label>("carouselContent_item");
//             item.text = title;
//             OnSelectCard = onSelect;
//         }

//         public void SetContent(Sprite sprite, OnSelect onSelect)
//         {
//             var item = this.CreateChild<Image>("carouselContent_item");
//             item.sprite = sprite;
//             OnSelectCard = onSelect;
//         }

//         public void SetContent(string title, Sprite sprite, OnSelect onSelect)
//         {
//             var image = this.CreateChild<Image>("carouselContent_item_image");
//             image.sprite = sprite;

//             var label = this.CreateChild<Label>("carouselContent_item_label");
//             label.text = title;
//             OnSelectCard = onSelect;
//         }
//     }

//     public partial class Carousel : VisualElement
//     {
//         VisualElement _content;
//         Button _btnRight;
//         Button _btnLeft;
//         Button _btnSelect;
//         OnSelect OnSelectCurrent;

//         public Carousel()
//         {
//             _content = this.CreateChild("carouselContent");

//             var nav = this.CreateChild("carouselNavigation");
//             _btnLeft = nav.CreateChild<Button>("carouselNavigation_btn_left", "carouselNavigation_btn");
//             _btnLeft.text = "<";
//             _btnSelect = nav.CreateChild<Button>("carouselNavigation_btn_select", "carouselNavigation_btn");
//             _btnSelect.text = "O";
//             _btnRight = nav.CreateChild<Button>("carouselNavigation_btn_right", "carouselNavigation_btn");
//             _btnRight.text = ">";

//             _btnLeft.clicked += GoLeft;
//             _btnSelect.clicked += () => OnSelectCurrent();
//             _btnRight.clicked += GoRight;
//         }

//         public void AddContent(string title, OnSelect onSelect = null)
//         {
//             _content
//                 .CreateChild<CarouselCard>("carouselContent_item")
//                 .SetContent(title, onSelect);

//             Reorder();
//         }

//         public void AddContent(Sprite sprite, OnSelect onSelect = null)
//         {
//             _content
//                 .CreateChild<CarouselCard>("carouselContent_item")
//                 .SetContent(sprite, onSelect);

//             Reorder();
//         }

//         public void AddContent(string title, Sprite sprite, OnSelect onSelect = null)
//         {
//             _content
//                 .CreateChild<CarouselCard>("carouselContent_item")
//                 .SetContent(title, sprite, onSelect);

//             Reorder();
//         }

//         void GoRight()
//         {
//             if (_btnRight.enabledSelf)
//                 _ = MoveRight();
//         }

//         async Task MoveRight()
//         {
//             _btnRight.SetEnabled(false);
//             Reorder(1);
//             await Task.Delay(400);
//             var next = _content.Children().ElementAt(0);
//             _content.RemoveAt(0);
//             _content.Add(next);
//             _btnRight.SetEnabled(true);
//         }

//         void GoLeft()
//         {
//             if (_btnLeft.enabledSelf)
//                 _ = MoveLeft();
//         }

//         async Task MoveLeft()
//         {
//             _btnLeft.SetEnabled(false);
//             Reorder(-1);
//             await Task.Delay(400);
//             var next = _content.Children().ElementAt(_content.childCount - 1);
//             _content.RemoveAt(_content.childCount - 1);
//             _content.Insert(0, next);
//             _btnLeft.SetEnabled(true);
//         }

//         void Reorder(int offset = 0)
//         {
//             var prev = _content.Children().FirstOrDefault(c => c.ClassListContains("previous"));
//             var curr = _content.Children().FirstOrDefault(c => c.ClassListContains("current"));
//             var next = _content.Children().FirstOrDefault(c => c.ClassListContains("next"));

//             prev?.RemoveFromClassList("previous");
//             curr?.RemoveFromClassList("current");
//             next?.RemoveFromClassList("next");

//             int currId = (_content.childCount / 2) + offset;
//             int prevId = currId - 1;
//             int nextId = currId + 1;

//             prev = _content.Children().ElementAtOrDefault(prevId);
//             prev?.AddToClassList("previous");

//             curr = _content.Children().ElementAtOrDefault(currId);
//             curr?.AddToClassList("current");

//             next = _content.Children().ElementAtOrDefault(nextId);
//             next?.AddToClassList("next");

//             OnSelectCurrent = (curr as CarouselCard).OnSelectCard;
//         }
//     }

// }
