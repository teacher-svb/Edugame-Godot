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

        public string CharacterName => this._name;

        [Export]
        public Godot.Collections.Dictionary<InputAction, Ability> CharacterAbilities { get; private set; }
        [Export]
        public BaseStats CharacterBaseStats { get; private set; }

        public InputActionBase[] InputActions => CharacterAbilities.Keys.ToArray();
    }
}