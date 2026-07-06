using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeView : Control
    {
        private ChallengeUI _challengeUi;

        public ChallengeUI ChallengeUI => _challengeUi;

        public override void _Ready() { }

        public async Task InitializeView(IMathChallenge challenge)
        {
            _challengeUi = this.FindAnyObjectByType<ChallengeUI>();
            Modulate = Colors.Transparent;
            Scale = Vector2.Zero;

            await Task.Yield();
        }

        public async Task ShowView(float duration = 0.2f)
        {
            await this.ScaleIn(duration);
            Show();
        }

        public async Task HideView(float duration = 0.2f)
        {
            await this.ScaleOut(duration);
            Hide();
        }

        public void Refresh(IMathChallenge challenge)
        {
            _challengeUi = ChallengeUIFactory.Build(challenge);
        }
    }
}
