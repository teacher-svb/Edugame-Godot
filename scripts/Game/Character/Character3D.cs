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

        // the characters in the tree, for TurnSpeakerToPlayer
        static readonly System.Collections.Generic.List<Character3D> _inTree = new();

        public override void _EnterTree()
        {
            _InitSubNodes();
            _inTree.Add(this);
        }

        public override void _ExitTree()
        {
            _inTree.Remove(this);
        }

        /// <summary>
        /// Turns the character that speaks a line toward the player: the nearest one in the tree with
        /// <paramref name="speaker"/> as its data, not the player itself. Nothing for a line without a character, one
        /// the player speaks, or a speaker that isn't in the level.
        /// </summary>
        public static void TurnSpeakerToPlayer(CharacterData speaker)
        {
            if (speaker == null || !IsInstanceValid(Player.Instance) || !Player.Instance.IsInsideTree())
                return;
            var player = Player.Instance.FindAncestorOfType<Character3D>();
            if (player == null)
                return;
            var nearest = _inTree
                .Where(c => c != player && IsInstanceValid(c) && c.IsInsideTree() && IsSameCharacter(c._characterData, speaker))
                .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(player.GlobalPosition))
                .FirstOrDefault();
            nearest?.FindAnyObjectByType<CharacterController3D>(includeSelf: true)?.FaceTowards(player.GlobalPosition);
        }

        // the same resource, or the same file (a duplicated or reloaded copy)
        static bool IsSameCharacter(CharacterData a, CharacterData b) =>
            a != null && (a == b || (!string.IsNullOrEmpty(a.ResourcePath) && a.ResourcePath == b.ResourcePath));

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