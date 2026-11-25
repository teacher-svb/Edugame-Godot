using Godot;
using TnT.EduGame.Challenges;

namespace TnT.EduGame.Doors
{
    /// <summary>
    /// A clue in the world for a door's challenge: shows <see cref="Text"/> filled from the round of
    /// <see cref="Source"/> (the door's <see cref="MathChallengeTrigger"/>) on <see cref="Label"/>, e.g. the
    /// number a key door wants. Reading the round fixes the trigger's seed, so the clue and the door show
    /// the same values; it redraws when the seed changes (save data bound after load, or set by the populator).
    /// </summary>
    [GlobalClass]
    public partial class CodeClue : Node3D
    {
        /// <summary>The challenge whose values this clue shows.</summary>
        [Export] public MathChallengeTrigger Source { get; set; }

        /// <summary>What the clue says, with <c>{param}</c> placeholders from the round (unknowns show as <c>?</c>).</summary>
        [Export(PropertyHint.MultilineText)] public string Text { get; set; } = "{answer}";

        [Export] public Label3D Label { get; set; }

        public override void _Ready()
        {
            if (Source == null || Label == null)
            {
                GD.PushWarning($"{Name}: CodeClue needs a Source and a Label");
                return;
            }
            Source.SeedChanged += Refresh;
            Refresh();
        }

        public override void _ExitTree()
        {
            if (Source != null)
                Source.SeedChanged -= Refresh;
        }

        void Refresh()
        {
            var round = Source.Round;
            Label.Text = round == null ? "" : round.Format(Text);
        }
    }
}
