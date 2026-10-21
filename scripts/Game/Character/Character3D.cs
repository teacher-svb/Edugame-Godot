using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class Character3D : CharacterBody3D
    {
        [Export]
        CharacterData _characterData;
        [Export]
        Camera3D _closeupCamera;
        public Stats Stats { get; private set; }
        public Attributes Attributes { get; private set; }
        CharacterStateManager _stateManager;

        [Signal] public delegate void StateCompletedEventHandler();

        public override void _Ready()
        {
            // _InitSubNodes();

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
            get { return this.PersistentId; }
            // set
            // {
            //     _characterData =
            //         ResourceFinder
            //             .FindObjectsOfTypeAll<CharacterData>()
            //             .FirstOrDefault(c => c. == value) ?? _characterData;
            // }
        }

        public CharacterData CharacterData { get => _characterData; }
        public Camera3D CloseupCamera { get => _closeupCamera; }

        public void LoadCharacter(string characterId)
        {
            this.PersistentId = characterId;
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

        // Completes when the idle is over. Returns at once while patrolling/following,
        // because Idling is ignored then and StateCompleted would never fire.
        // Note: StateCompleted (used here and in PatrolAsync) only fires once the state stack
        // is back to its base state, so these only complete when nothing else is pushed on top.
        public async Task IdleAsync(int durationMs)
        {
            if (_stateManager.IsAutonomousBehaviorActive)
                return;

            var completed = ToSignal(this, SignalName.StateCompleted);
            SetStateToIdle(durationMs);
            await completed;
        }

        public void SetStateToPatrolling(params Node[] patrolTargets)
        {
            _stateManager.StartPatrol(patrolTargets);
        }

        // Completes when the patrol ends; never completes for a looping patrol
        public async Task PatrolAsync(Node3D[] waypoints)
        {
            var completed = ToSignal(this, SignalName.StateCompleted);
            SetStateToPatrolling(waypoints);
            await completed;
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

        public Task PopCurrentStateAsync() => _stateManager.Pop();

    }
}