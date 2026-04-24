using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;
using TnT.Systems;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    public partial class CharacterStateManager : AbstractStateStack
    {
        public ICharacterController Controller { get; private set; }
        public CharacterAgent3DPatrol PatrolAgent { get; private set; }
        public CharacterAgent3DFollow FollowAgent { get; private set; }

        private readonly List<BaseCharacterState> _registeredStates = new();

        public override void _Ready()
        {
            Controller = this.FindAncestorOfType<CharacterController3D>();
            if (Controller == null)
                Controller = this.FindAncestorOfType<CharacterController2D>();

            var parent = GetParent();
            PatrolAgent = parent.FindAnyObjectByType<CharacterAgent3DPatrol>();
            FollowAgent = parent.FindAnyObjectByType<CharacterAgent3DFollow>();

            var idle = _registeredStates.OfType<CharacterStateIdle>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateIdle child node");

            Push(idle.GetState(new()));
        }

        internal void RegisterState(BaseCharacterState state)
        {
            _registeredStates.Add(state);
        }

        // Pop the current behavior state to return to idle.
        // Call this before switching to a different behavior.
        public new void Pop() => base.Pop();

        public void Follow(Node3D target)
        {
            var state = _registeredStates.OfType<CharacterStateFollowing>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateFollowing child node");
            Push(state.GetState(new() { Target = target }));
        }

        public void StartMoving()
        {
            var state = _registeredStates.OfType<CharacterStateMoving>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStateMoving child node");
            Push(state.GetState(new()));
        }

        public void StartPatrol()
        {
            var state = _registeredStates.OfType<CharacterStatePatrolling>().FirstOrDefault()
                ?? throw new Exception("CharacterStateManager requires a CharacterStatePatrolling child node");
            Push(state.GetState(new()));
        }
    }
}
