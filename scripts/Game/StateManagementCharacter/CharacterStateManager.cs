using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.QuestSystem;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    public partial class CharacterStateManager : AbstractStateStack, IQuestReactionObject
    {
        private readonly List<BaseCharacterState> _registeredStates = new();
        private bool _autonomousBehaviorActive = false;
        public bool IsAutonomousBehaviorActive => _autonomousBehaviorActive;

        NavigationAgent3D _agent;
        CharacterController3D _controller;

        public event Action ReactionCompleted
    {
        add    => Connect(SignalName.SequenceCompleted, Callable.From(value));
        remove => Disconnect(SignalName.SequenceCompleted, Callable.From(value));
    }

        public enum stateTypes
        {
            idle,
            patrol,
            follow,
            input
        }

        [Signal] public delegate void SequenceCompletedEventHandler();


        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Pausable;
            _controller = this.FindAncestorOfType<CharacterController3D>();
            _agent = _controller.FindAnyObjectByType<NavigationAgent3D>();
            Idling();
        }

        internal void RegisterState(BaseCharacterState state)
        {
            _registeredStates.Add(state);
        }

        public override async Task Pop()
        {
            _autonomousBehaviorActive = false;
            await base.Pop();
            if (StateCount == 1)
                EmitSignal(SignalName.SequenceCompleted);
        }

        public void Idling(int durationMs = -1)
        {
            GD.Print(durationMs);
            if (_autonomousBehaviorActive) return;
            var idle = _registeredStates.OfType<CharacterStateIdle>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateIdle child node");
            Push(idle.GetState(new() { durationMs = durationMs }));
        }

        public void Follow(Node3D target)
        {
            var state = _registeredStates.OfType<CharacterStateFollowing>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateFollowing child node");
            _autonomousBehaviorActive = true;
            Push(state.GetState(new() { agent = _agent, cc = _controller, Target = target }));
        }

        public void StartInput(InputAction2D moveAction, InputAction jumpAction)
        {
            if (_autonomousBehaviorActive) return;
            var state = _registeredStates.OfType<CharacterStateMoveInput>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateMoving child node");
            Push(state.GetState(new() { agent = _agent, cc = _controller, moveAction = moveAction, jumpAction = jumpAction }));
        }

        public void StartPatrol(params Node[] patrolTargets)
        {
            var state = _registeredStates.OfType<CharacterStatePatrolling>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStatePatrolling child node");
            _autonomousBehaviorActive = true;
            Push(state.GetState(new() { agent = _agent, cc = _controller, targets = patrolTargets.Where(t => t is Node3D).Select(t => t as Node3D).ToArray() }));
        }


    }
}
