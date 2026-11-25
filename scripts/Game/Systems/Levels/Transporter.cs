using Godot;
using System;
using TnT.EduGame.GameStates;
using TnT.Extensions;
using TnT.EduGame.Characters;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// Old-style exit on an Area3D/Area2D (connect its <c>body_entered</c> to <see cref="_OnBodyEntered"/>): the player
    /// walking in is moved to <see cref="_destination"/> in this level, or loads <see cref="_sceneToLoadPath"/>. Used once:
    /// a scene load for good, a move within the level until the player has left this area (several collision shapes
    /// entering at once move only once). Arriving inside another transporter would send the player on, so keep
    /// destinations out of transporter areas. New exits use <see cref="LevelExit"/>.
    /// </summary>
    [GlobalClass]
    public partial class Transporter : Node
    {
        [Export] protected Transporter _destination;
        [Export(PropertyHint.File, "*.tscn,*.scn")]
        protected string _sceneToLoadPath;
        // [Export] protected Resource _sceneToLoad;

        // world position: the loader's MoveTo places the player in world space
        protected Vector3 TransporterLocation
        {
            get
            {
                var pos = this.Get("global_position");
                return pos.VariantType switch
                {
                    Variant.Type.Vector2 => pos.AsVector2().ToVector3(),
                    Variant.Type.Vector3 => pos.AsVector3(),
                    _ => throw new InvalidOperationException($"{GetType().Name} must inherit from Node2D or Node3D")
                };
            }
        }
        bool _used;

        public override void _Ready()
        {
            // leaving clears the guard; only body_entered is connected in the scenes. By name: the script is a Node
            // on an Area3D/Area2D, so the C# area events aren't there
            if (HasSignal(Area3D.SignalName.BodyExited))
                Connect(Area3D.SignalName.BodyExited, Callable.From<Node>(OnBodyExited));
        }

        public void _OnBodyEntered(Node other)
        {
            if (_used || !Player.IsPlayer(other))
                return;

            if (_destination != null)
            {
                _used = true;   // until the player has left this area
                StateManagerGame.Instance.LoadLocation(_destination.TransporterLocation);
            }
            else if (!string.IsNullOrEmpty(_sceneToLoadPath))
            {
                _used = true;   // for good: this level is unloaded
                StateManagerGame.Instance.LoadScene(_sceneToLoadPath, TransporterLocation);
            }
        }

        void OnBodyExited(Node other)
        {
            if (_destination != null && Player.IsPlayer(other))
                _used = false;
        }
    }
}
