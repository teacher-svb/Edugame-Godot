
using System;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Anything that lets the player supply a value for one of the round's unknowns.
    // Implemented by every input widget, and by visualization widgets that double
    // as input (e.g. FormulaRing, where spinning the rings is the answer).
    public interface IChallengeInput
    {
        event Action<string, int> ValueAssigned;
        // the player took a value back (e.g. deselected a picked number)
        event Action<string> ValueCleared;
    }

    // Uniform contract for anything placed in ChallengeUI's input slot: whenever
    // the player supplies a number for one of the round's unknowns, report it.
    // How that number is produced (typed, dialed, picked) is entirely up to the
    // concrete widget, which is free to be a scene of its own with hand-placed
    // child controls (see Scenes/UI/ChallengeInputs/*.tscn).
    public partial class ChallengeInputWidget : Control, IChallengeInput
    {
        public event Action<string, int> ValueAssigned;
        public event Action<string> ValueCleared;

        // Set by the builder right after Instantiate(), before the widget enters
        // the tree, so they're available to subclasses by the time their _Ready() runs.
        protected IMathChallenge Challenge { get; private set; }
        protected ChallengeRound Round { get; private set; }

        public void Init(IMathChallenge challenge, ChallengeRound round)
        {
            Challenge = challenge;
            Round = round;
        }

        public void EmitValueAssigned(string paramName, int value) => ValueAssigned?.Invoke(paramName, value);
        public void EmitValueCleared(string paramName) => ValueCleared?.Invoke(paramName);
    }
}
