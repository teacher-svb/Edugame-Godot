using Godot;
using System;
using System.Collections.Generic;
using TnT.EduGame.GameState;
using TnT.Extensions;
using TnT.Systems;

[GlobalClass]
public partial class Transporter : Node
{
    [Export] protected Transporter _destination;
    [Export(PropertyHint.File, "*.tscn,*.scn")]
    protected string _sceneToLoadPath;
    // [Export] protected Resource _sceneToLoad;

    protected Vector3 TransporterLocation
    {
        get
        {
            var pos = this.Get("position");
            return pos.VariantType switch
            {
                Variant.Type.Vector2 => pos.AsVector2().ToVector3(),
                Variant.Type.Vector3 => pos.AsVector3(),
                _ => throw new InvalidOperationException($"{GetType().Name} must inherit from Node2D or Node3D")
            };
        }
    }
    public void _OnBodyEntered(Node other)
    {
        if (other.FindAnyObjectByType<Player>() == null)
            return;


        if (_destination != null)
            StateManagerGame.Instance.LoadLocation(_destination.TransporterLocation);
        else if (_sceneToLoadPath != "")
            StateManagerGame.Instance.LoadScene(_sceneToLoadPath, TransporterLocation);
    }
}