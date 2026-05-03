using System;
using System.Collections.Generic;
using System.Linq;
using TnT.Systems.Persistence;
using Godot;
using TnT.Systems;
using TnT.Extensions;
using TnT.Input;
using TnT.EduGame.CharacterState;
using TnT.EduGame.QuestSystem;

namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class Character3D : CharacterBody3D, IBind<CharacterSaveData>, IQuestReactionObject
    {
        [Export]
        CharacterData _characterData;
        public Stats Stats { get; private set; }
        public Attributes Attributes { get; private set; }
        CharacterStateManager _stateManager;

        public event Action ReactionCompleted
        {
            add => Connect(SignalName.StateCompleted, Callable.From(value));
            remove => Disconnect(SignalName.StateCompleted, Callable.From(value));
        }


        [Signal] public delegate void StateCompletedEventHandler();

        public override void _Ready()
        {
            _InitSubNodes();

            if (this._characterData != null)
            {
                this.Stats = new Stats(this._characterData.CharacterBaseStats);
                this.Attributes = new Attributes(this.Stats);
            }

            this._characterData?.CharacterAbilities.ForEach(a => a.Key.Enabled = true);
        }

        public override void _EnterTree()
        {
            _InitSubNodes();
        }

        private void _InitSubNodes()
        {
            // this._controller = this.FindAnyObjectByType<CharacterController3D>();
            this._stateManager = this.FindAnyObjectByType<CharacterStateManager>();
            this._stateManager.SequenceCompleted += () => EmitSignal(SignalName.StateCompleted);
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
        }

        public override void _Process(double delta)
        {
            Stats?.Mediator.Update(delta);
            Attributes?.Mediator?.Update(delta);
            if (_saveData != null)
                _saveData.position = this.Position;

            this._characterData?.CharacterAbilities
                .Where(a => a.Key.Triggered)
                .ForEach(a => a.Value.Activate(this, this));
        }

        public void SetStateToIdle(int durationMs)
        {
            _stateManager.Idling(durationMs);
        }

        public void SetStateToPatrolling(params Node[] patrolTargets)
        {
            _stateManager.StartPatrol(patrolTargets);
        }

        public void SetStateToInput(InputAction2D moveAction, InputAction jumpAction)
        {
            _stateManager.StartInput(moveAction, jumpAction);
        }

        public void SetStateToFollow(Node3D target)
        {
            _stateManager.Follow(target);
        }

        public void PopCurrentState()
        {
            _ = _stateManager.Pop();
        }

        #region SAVE/LOAD

        [Export]
        CharacterSaveData _saveData;

        public UniqueId UniqueId { get; set; } = new() { Id = Guid.NewGuid().ToString() };

        public void Bind(CharacterSaveData data)
        {
            _saveData = data;
            _saveData.Id = UniqueId.Id;
            this.Position = _saveData.position;
            LoadCharacter(_saveData.characterId);
        }
        #endregion
    }
}