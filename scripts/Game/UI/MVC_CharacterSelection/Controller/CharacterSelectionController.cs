
using System;
using System.Threading.Tasks;
using TnT.EduGame.Characters;
using UnityEngine;
using UnityEngine.Events;

namespace TnT.Systems.UI
{
    public class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField]
        public CharacterSelectionView view = new();
        [SerializeField]
        public CharacterSelectionModel model = new();
        [SerializeField]
        UnityEvent<CharacterData> onSelectCharacter;
        void Start()
        {
            Initialize(onSelectCharacter);
        }

        async void Initialize(UnityEvent<CharacterData> onSelectCharacter)
        {
            await view.InitializeView();

            for (var i = 0; i < Mathf.Max(6, model.Count); ++i)
            {
                view.AddCharacter(model.Get(i), onSelectCharacter);
            }
        }
    }
}
