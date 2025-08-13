using System;
using System.Collections.Generic;
using System.Linq;
using TnT.Systems.Persistence;
using Godot;
using TnT.Systems;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.Characters
{
    public partial class Character : Node, IBind<CharacterSaveData>
    {
        [Export]
        BaseStats _baseStats;
        public Stats Stats { get; private set; }
        public Attributes Attributes { get; private set; }
        [Export]
        CharacterData _characterData;
        CharacterController2D _controller;


        public string CharacterId
        {
            get { return _characterData.Id; }
            set { _characterData = ResourceFinder.FindObjectsOfTypeAll<CharacterData>().FirstOrDefault(c => c.Id == value); }
        }
        public void LoadCharacter(string characterId)
        {
            CharacterId = characterId;
            if (_characterData == null)
                return;

            _saveData.characterId = CharacterId;
            _controller.FindAnyObjectByType<AnimatedSprite2D>().SpriteFrames = this._characterData.animatorController;
        }

        // [Export] Dictionary<InputActionReference, Ability> _abilities = new();
        [Export]
        public Godot.Collections.Dictionary<InputActionReference, Ability> _abilities = new();

        public override void _Ready()
        {
            // this._baseStats = Instantiate(_baseStats);
            this.Stats = new Stats(this._baseStats);
            this.Attributes = new Attributes(this.Stats);

            // _abilities.ForEach(a => a.Key.action.Enable());
        }

        public override void _Process(double delta)
        {
            Stats.Mediator.Update(delta);
            Attributes.Mediator?.Update(delta);
            _saveData.position = _controller.Position;

            // _abilities.ForEach(a =>
            // {
            //     if (a.Key.action.triggered)
            //     {
            //         a.Value.Activate(this, this);
            //     }
            // });
            GD.Print(Attributes.ToString());
        }

        #region SAVE/LOAD

        [Export]
        CharacterSaveData _saveData = null;

        public UniqueId UniqueId { get; set; } = new() { Id = Guid.NewGuid().ToString() };

        public void Bind(CharacterSaveData data)
        {
            _saveData = data;
            _saveData.Id = UniqueId.Id;
            _controller.Position = _saveData.position;
            LoadCharacter(_saveData.characterId);
        }
        #endregion
    }
}