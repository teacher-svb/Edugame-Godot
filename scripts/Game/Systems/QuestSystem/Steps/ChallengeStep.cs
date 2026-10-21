using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;

namespace TnT.EduGame.QuestSystem
{
    // Shows the challenge and waits for the answer
    [GlobalClass]
    public partial class ChallengeStep : QuestStep
    {
        [Export] MathChallengeTrigger _trigger;

        public override async Task Run(QuestObjective objective)
        {
            // A failed challenge resets and restarts its objective, which runs this listener
            // again; stop this run so the remaining steps don't execute twice.
            if (!await _trigger.RunChallengeAsync())
                throw new OperationCanceledException();
        }
    }
}
