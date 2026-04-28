using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    public partial class CharacterStateManager : AbstractStateStack
    {
        private readonly List<BaseCharacterState> _registeredStates = new();

        NavigationAgent3D _agent;
        CharacterController3D _controller;

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

        // Pop the current behavior state to return to idle.
        // Call this before switching to a different behavior.
        public new void Pop() => base.Pop();

        public void Idling()
        {
            var idle = _registeredStates.OfType<CharacterStateIdle>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateIdle child node");

            Push(idle.GetState(new()));
        }

        public void Follow(Node3D target)
        {
            var state = _registeredStates.OfType<CharacterStateFollowing>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateFollowing child node");
            Push(state.GetState(new() { agent = _agent, cc = _controller, Target = target }));
        }

        public void StartInput(InputAction2D moveAction, InputAction jumpAction)
        {
            var state = _registeredStates.OfType<CharacterStateMoveInput>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateMoving child node");
            Push(state.GetState(new() {agent = _agent, cc = _controller, moveAction = moveAction, jumpAction = jumpAction}));
        }

        public void StartPatrol(Node3D[] patrolTargets)
        {
            var state = _registeredStates.OfType<CharacterStatePatrolling>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStatePatrolling child node");
            Push(state.GetState(new() { agent = _agent, cc = _controller, targets = patrolTargets }));
        }
    }
}
