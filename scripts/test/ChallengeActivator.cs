using Godot;
using System;
using System.Collections.Generic;
using TnT.EduGame.GameState;
using TnT.EduGame.Question;
using TnT.Extensions;

// [Tool]
[GlobalClass]
public partial class ChallengeActivator : Area2D
{
    [Export]
    MathChallenge _challenge;
    public void _OnBodyEntered(Node2D other)
    {
        if (other is Player)
            StateManagerGame.Instance.ShowChallenge(_challenge);
    }
}
