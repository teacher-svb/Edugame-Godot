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
        CharacterData _characterData;
        public Stats Stats { get; private set; }
        public Attributes Attributes { get; private set; }
        CharacterController2D _controller;

        public override void _EnterTree()
        {
            this._controller = GetParent<CharacterController2D>();
        }

        public string CharacterId
        {
            get { return _characterData.Id; }
            set
            {
                _characterData =
                    ResourceFinder
                        .FindObjectsOfTypeAll<CharacterData>()
                        .FirstOrDefault(c => c.Id == value) ?? _characterData;
            }
        }
        public void LoadCharacter(string characterId)
        {
            CharacterId = characterId;
            if (_characterData == null)
                return;

            _saveData.characterId = CharacterId;
            _controller.FindAnyObjectByType<AnimatedSprite2D>().SpriteFrames = this._characterData.animatorController;
        }

        public override void _Ready()
        {
            this._controller = GetParent<CharacterController2D>();
            this.Stats = new Stats(this._characterData.CharacterBaseStats);
            this.Attributes = new Attributes(this.Stats);

            this._characterData.CharacterAbilities.ForEach(a => a.Key.Enabled = true);
        }

        public override void _Process(double delta)
        {
            Stats.Mediator.Update(delta);
            Attributes.Mediator?.Update(delta);
            _saveData.position = _controller.Position;

            if (this._characterData != null)
            {
                this._characterData.CharacterAbilities.ForEach(a =>
                    {
                        if (a.Key.Triggered)
                        {
                            a.Value.Activate(this, this);
                        }
                    });
            }
            // GD.Print(Attributes.ToString());
        }

        #region SAVE/LOAD

        [Export]
        CharacterSaveData _saveData;

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