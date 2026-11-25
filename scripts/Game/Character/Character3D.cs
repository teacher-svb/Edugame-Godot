using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.EduGame.Characters.States;

using TnT.Systems.OcclusionFade;
namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class Character3D : CharacterBody3D, IOcclusionExempt
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
            if (this._characterData != null)
            {
                this.Stats = new Stats(this._characterData.CharacterBaseStats);
                this.Attributes = new Attributes(this.Stats);
            }

            // most CharacterData resources store CharacterAbilities = null (no abilities)
            this._characterData?.CharacterAbilities?.ForEach(a => a.Key.Enabled = true);
        }

        public override void _EnterTree()
        {
            _InitSubNodes();
        }

        // _EnterTree runs again after every re-parenting / re-adding: look up and subscribe only once (finding 4.15)
        private void _InitSubNodes()
        {
            if (this._stateManager != null)
                return;
            this._stateManager = this.FindAnyObjectByType<CharacterStateManager>(includeSelf: true);
            this._stateManager.SequenceCompleted += OnSequenceCompleted;
        }

        void OnSequenceCompleted() => EmitSignal(SignalName.StateCompleted);

        public string CharacterId => this.PersistentId;

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
            // only with modifiers: nothing to tick otherwise (the usual case: no ability adds any)
            if (Stats?.Mediator.HasModifiers == true)
                Stats.Mediator.Update(delta);
            if (Attributes?.Mediator.HasModifiers == true)
                Attributes.Mediator.Update(delta);
            if (_saveData != null)
                _saveData.position = this.Position;

            this._characterData?.CharacterAbilities?
                .Where(a => a.Key.Triggered)
                .ForEach(a => a.Value.Activate(this, this));
        }

        public void SetStateToIdle(int durationMs)
        {
            _stateManager.Idling(durationMs);
        }

        // Completes when this idle leaves the stack (its own state: it no longer waits for the whole stack to be back
        // at its base). Returns at once while patrolling/following, because Idling is ignored then.
        public Task IdleAsync(int durationMs) => _stateManager.Idling(durationMs);

        public void SetStateToPatrolling(params Node[] patrolTargets)
        {
            _stateManager.StartPatrol(patrolTargets);
        }

        // Completes when this patrol ends: true at the end of its route (unreachable waypoints are skipped with a
        // warning), false when it was stopped first (replaced by another patrol or follow, popped). A looping patrol
        // only completes when stopped.
        public Task<bool> PatrolAsync(Node3D[] waypoints) => _stateManager.StartPatrol(waypoints);

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