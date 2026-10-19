using Godot;

namespace TnT.EduGame.Question
{
    // How the player supplies values for a round's unknowns. Concrete subclasses
    // carry their own settings and point at the scene that renders them.
    [GlobalClass, Tool]
    public abstract partial class ChallengeInput : ChallengeElement
    {
    }
}
