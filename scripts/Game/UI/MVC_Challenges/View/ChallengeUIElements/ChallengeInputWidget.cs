
using System;
using Godot;

namespace TnT.EduGame.UI
{
    // Anything that lets the player supply a value for one of the round's unknowns.
    // Implemented by every input widget, and by visualization widgets that double
    // as input (e.g. KeyWidget, where cutting the teeth is the answer).
    public interface IChallengeInput
    {
        event Action<string, int> ValueAssigned;
        // the player took a value back (e.g. deselected a picked number)
        event Action<string> ValueCleared;

        // Can the player give `param` a value here? Input widgets take every unknown; a
        // visualization that doubles as input only its own params (ChallengeUI checks this).
        bool Handles(string param) => true;
    }

    // Uniform contract for anything placed in ChallengeUI's input slot: whenever
    // the player supplies a number for one of the round's unknowns, report it.
    // How that number is produced (typed, dialed, picked) is entirely up to the
    // concrete widget, which is free to be a scene of its own with hand-placed
    // child controls (see Scenes/UI/ChallengeInputs/*.tscn).
    public partial class ChallengeInputWidget : ChallengeWidget, IChallengeInput
    {
        public event Action<string, int> ValueAssigned;
        public event Action<string> ValueCleared;

        public void EmitValueAssigned(string paramName, int value) => ValueAssigned?.Invoke(paramName, value);
        public void EmitValueCleared(string paramName) => ValueCleared?.Invoke(paramName);

        // focus a control when the challenge opens (see FocusTarget)
        protected Control FirstFocus { get; set; }
        public override Control FocusTarget => FirstFocus;
    }
}
