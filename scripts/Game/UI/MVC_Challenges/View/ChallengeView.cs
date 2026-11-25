using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Challenges;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class ChallengeView : Control
    {
        private ChallengeUI _challengeUi;

        public ChallengeUI ChallengeUI => _challengeUi;

        public void InitializeView()
        {
            _challengeUi = this.FindAnyObjectByType<ChallengeUI>(includeSelf: true);
            Modulate = Colors.Transparent;
            Scale = Vector2.Zero;
            // hidden until the first challenge: no clicks, no Enter shortcut on Submit
            Hide();
        }

        // visible (and on top) before the scale-in, or from the 2nd opening on it would run hidden
        public async Task ShowView(float duration = 0.2f)
        {
            Show();
            ZIndex = 1000;
            _challengeUi.FocusInput();
            await this.ScaleIn(duration);
        }

        public async Task HideView(float duration = 0.2f)
        {
            ZIndex = -1;
            // no second submit or input while it scales out
            _challengeUi.SetSubmitEnabled(false);
            _challengeUi.ReleaseInputFocus();
            // not when a ShowView took over during the scale-out (UITweens resumes the replaced awaiter with false)
            if (await this.ScaleOut(duration))
                Hide();
        }

        public void Refresh(MathChallenge challenge, ChallengeRound round)
        {
            _challengeUi.ShowRound(challenge, round);
        }
    }
}
