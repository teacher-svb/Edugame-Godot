using Godot;
using System;
using System.Collections.Generic;
using TnT.EduGame.GameState;
using TnT.Extensions;

// [Tool]
[GlobalClass]
public partial class Transporter : Area2D
{
    [Export] Transporter _destination;
    [Export] Resource _sceneToLoad;
    public void _OnBodyEntered(Node2D other)
    {
        if (other is Player)
            _destination?.MoveHere();
    }

    public void MoveHere()
    {
        if (_sceneToLoad == null)
        {
            StateManagerGame.Instance.LoadLocation(this.Position);
            return;
        }

        GD.Print(this.Position);
        StateManagerGame.Instance.LoadScene(_sceneToLoad, this.Position);
    }
}
