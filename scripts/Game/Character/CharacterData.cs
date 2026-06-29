using System;
using System.Linq;
using Godot;
using TnT.Input;

namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class CharacterData : Resource, IInputActionable
    {
        [Export]
        string _name;
        [Export]
        Texture2D _face;

        public string CharacterName => this._name;
        public Texture2D CharacterFace => this._face;

        [Export]
        public Godot.Collections.Dictionary<InputAction, Ability> CharacterAbilities { get; private set; }
        [Export]
        public BaseStats CharacterBaseStats { get; private set; }

        public InputActionBase[] InputActions => CharacterAbilities.Keys.ToArray();
    }
}