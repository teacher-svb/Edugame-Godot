
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [Serializable]
    public partial class ChallengeView : Resource
    {
        Control _root;
        Node container;
        private ChallengeUI _challengeUi;

        public ChallengeUI ChallengeUI => _challengeUi;

        public async Task InitializeView(Control root, IMathChallenge challenge)
        {
            _root = root;
            _root.Clear();

            container = _root.CreateChild<Control>("container");

            await Task.Yield();

            Refresh(challenge);
        }

        public void Show()
        {
            // container.AddToClassList("opened");
            _root.Show();
        }

        public void Hide()
        {
            // container.RemoveFromClassList("opened");
            _root.Hide();
        }

        public void Refresh(IMathChallenge challenge)
        {
            if (_challengeUi != null && container.FindAnyObjectByType<ChallengeUI>() != null)
                container.RemoveChild(_challengeUi);

            _challengeUi = ChallengeUIFactory.Build(challenge);
        }
    }

}
