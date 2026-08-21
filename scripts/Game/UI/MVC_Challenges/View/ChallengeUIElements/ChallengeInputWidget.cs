
using System;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Uniform contract for anything placed in ChallengeUI's challenge container:
    // it takes up space and, whenever the player supplies a number for a formula
    // param, reports it. How that number is produced (typed, dialed, picked) is
    // entirely up to the concrete widget, which is free to be a scene of its own
    // with hand-placed child controls (see Scenes/UI/ChallengeStrategies/*.tscn).
    public partial class ChallengeInputWidget : Control
    {
        public event Action<string, int> ValueAssigned;

        // Set by the factory right after Instantiate(), before the widget enters
        // the tree, so it's available to subclasses by the time their _Ready() runs.
        protected IMathChallenge Challenge { get; private set; }

        public void Init(IMathChallenge challenge) => Challenge = challenge;

        public void EmitValueAssigned(string paramName, int value) => ValueAssigned?.Invoke(paramName, value);
    }
}
