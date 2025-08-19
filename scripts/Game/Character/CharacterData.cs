using System;
using Godot;
using TnT.Input;

namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class CharacterData : Resource
    {
        [Export]
        string _id = Guid.NewGuid().ToString();
        [Export]
        string _name;
        [Export]
        SpriteFrames _animatorController;
        [Export]
        Texture _face;
        [Export]
        Texture _body;

        public string CharacterName => this._name;
        public Texture CharacterFace => this._face;
        public Texture CharacterBody => this._body;
        public string Id => this._id;
        public SpriteFrames animatorController => this._animatorController;

        [Export]
        public Godot.Collections.Dictionary<InputAction, Ability> CharacterAbilities { get; private set; }
        [Export]
        public BaseStats CharacterBaseStats { get; private set; }
    }
}