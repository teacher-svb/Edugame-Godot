
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeView : Control
    {
        Node container;
        private ChallengeUI _challengeUi;

        public ChallengeUI ChallengeUI => _challengeUi;

        public async Task InitializeView(IMathChallenge challenge)
        {

            container = this.CreateChild<Control>("container");

            await Task.Yield(); 

            Refresh(challenge);
        }

        public void ShowView()
        {
            // container.AddToClassList("opened");
            this.Show();
        }

        public void HideView()
        {
            // container.RemoveFromClassList("opened");
            this.Hide();
        }

        public void Refresh(IMathChallenge challenge)
        {
            if (_challengeUi != null && container.FindAnyObjectByType<ChallengeUI>() != null)
                container.RemoveChild(_challengeUi);

            _challengeUi = ChallengeUIFactory.Build(challenge);
        }
    }

}
