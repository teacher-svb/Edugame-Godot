
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeView : Control
    {
        private ChallengeUI _challengeUi;

        public ChallengeUI ChallengeUI => _challengeUi;

        public override void _Ready()
        {
        }

        public async Task InitializeView(IMathChallenge challenge)
        {

            _challengeUi = this.FindAnyObjectByType<ChallengeUI>();
            this.Modulate = Colors.Transparent;
            this.Scale = new Vector2(0, 0);

            await Task.Yield();

            // Refresh(challenge);
        }

        public async Task ShowView(float duration = .2f)
        {
            var startColor = this.Modulate;
            var targetColor = Colors.White;
            var startScale = this.Scale;
            var targetScale = new Vector2(1, 1);

            await foreach (var t in Easings.Easings.Animate(duration, Ease.EaseOutCubic))
            {
                this.Modulate = startColor.Lerp(targetColor, t);
                this.Scale = startScale.Lerp(targetScale, t);
            }

            this.Show();
        }

        public async Task HideView(float duration = .2f)
        {
            var startColor = this.Modulate;
            var targetColor = Colors.Transparent;
            var startScale = this.Scale;
            var targetScale = new Vector2(0, 0);

            await foreach (var t in Easings.Easings.Animate(duration, Ease.EaseOutCubic))
            {
                this.Modulate = startColor.Lerp(targetColor, t);
                this.Scale = startScale.Lerp(targetScale, t);
            }

            this.Hide();
        }

        public void Refresh(IMathChallenge challenge)
        {
            // if (_challengeUi != null && container.FindAnyObjectByType<ChallengeUI>() != null)
            //     container.RemoveChild(_challengeUi);

            _challengeUi = ChallengeUIFactory.Build(challenge);
        }
    }

}
