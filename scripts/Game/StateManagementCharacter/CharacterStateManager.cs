using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems.State;

namespace TnT.EduGame.Characters.States
{
    /// <summary>
    /// A character's state stack: the base idle (never popped), with on top of it a timed idle, the player's move
    /// input, or one autonomous behaviour (a patrol or a follow).
    /// </summary>
    /// <remarks>
    /// Autonomous behaviour is replaced, not stacked: starting a patrol or follow while one runs exits the old one
    /// first (its speed and agent settings are put back, then the new one starts from them). Its awaiter still
    /// completes: <see cref="StartPatrol"/>'s task with false ("stopped before the end"), not cancelled, so a quest
    /// reaction waiting on it goes on with its next steps instead of hanging (and keeping Save disabled). While a
    /// patrol or follow runs, <see cref="Idling"/> and <see cref="StartInput"/> are ignored.
    /// </remarks>
    public partial class CharacterStateManager : AbstractStateStack
    {
        private readonly List<BaseCharacterState> _registeredStates = new();
        BaseState _autonomous;   // the patrol or follow on the stack (at most one), until its OnExit
        BaseState _moveInput;    // the move input state on the stack, until its OnExit
        public bool IsAutonomousBehaviorActive => _autonomous != null;

        NavigationAgent3D _agent;
        CharacterController3D _controller;

        /// <summary>A pop brought the stack back to its base state (not emitted when there was nothing to pop).</summary>
        [Signal] public delegate void SequenceCompletedEventHandler();


        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Pausable;
            _controller = this.FindAncestorOfType<Character3D>().FindAnyObjectByType<CharacterController3D>(includeSelf: true);
            _agent = this.FindAncestorOfType<Character3D>().FindAnyObjectByType<NavigationAgent3D>(includeSelf: true);
            Idling();
        }

        internal void RegisterState(BaseCharacterState state)
        {
            _registeredStates.Add(state);
        }

        /// <summary>Pops the top state; nothing (and no <see cref="SequenceCompleted"/>) when only the base is left.</summary>
        public override Task Pop()
        {
            if (StateCount <= 1)
                return Task.CompletedTask;
            return PopAndNotify(Top);
        }

        async Task PopAndNotify(BaseState state)
        {
            if (!Contains(state))
                return;
            await Pop(state);
            // resumes a frame later: the character may have been freed meanwhile (scene change)
            if (IsInstanceValid(this) && StateCount == 1)
                EmitSignal(SignalName.SequenceCompleted);
        }

        /// <summary>
        /// Idles for <paramref name="durationMs"/> (negative: for good). The task completes when that idle leaves the
        /// stack; at once when it is ignored because a patrol or follow runs.
        /// </summary>
        public Task Idling(int durationMs = -1)
        {
            if (IsAutonomousBehaviorActive)
                return Task.CompletedTask;
            var idle = Registered<CharacterStateIdle>();
            var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Push(idle.GetState(new() { durationMs = durationMs, ended = () => ended.TrySetResult() }));
            return ended.Task;
        }

        /// <summary>Follows <paramref name="target"/> until popped or replaced; the task completes when it stops.</summary>
        public Task Follow(Node3D target)
        {
            var state = Registered<CharacterStateFollowing>();
            var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            BaseState followState = null;
            followState = state.GetState(new()
            {
                agent = _agent,
                cc = _controller,
                Target = target,
                ended = () =>
                {
                    AutonomousEnded(followState);
                    ended.TrySetResult();
                },
            });
            StartAutonomous(followState, "follow");
            return ended.Task;
        }

        /// <summary>Pushes the move input state, unless it is on the stack already or anything else is on top of the
        /// base state (a timed idle, a patrol or follow).</summary>
        public void StartInput(InputAction2D moveAction, InputAction jumpAction)
        {
            if (IsAutonomousBehaviorActive || _moveInput != null || StateCount > 1)
                return;
            var state = Registered<CharacterStateMoveInput>();
            BaseState inputState = null;
            inputState = state.GetState(new()
            {
                agent = _agent,
                cc = _controller,
                moveAction = moveAction,
                jumpAction = jumpAction,
                ended = () =>
                {
                    if (_moveInput == inputState)
                        _moveInput = null;
                },
            });
            _moveInput = inputState;
            Push(inputState);
        }

        /// <summary>Pops the move input state wherever it is; nothing else (finding 4.11).</summary>
        public void StopInput()
        {
            if (_moveInput != null)
                _ = PopAndNotify(_moveInput);
        }

        /// <summary>
        /// Walks the waypoints (see <see cref="CharacterStatePatrolling"/>). The task completes when the patrol leaves
        /// the stack: true when it walked its route to the end, false when it was stopped first (replaced, popped,
        /// unwound). A looping patrol only completes when stopped.
        /// </summary>
        public Task<bool> StartPatrol(params Node[] patrolTargets)
        {
            var state = Registered<CharacterStatePatrolling>();
            var ended = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            BaseState patrolState = null;
            patrolState = state.GetState(new()
            {
                agent = _agent,
                cc = _controller,
                targets = patrolTargets.OfType<Node3D>().ToArray(),
                ended = reachedEnd =>
                {
                    AutonomousEnded(patrolState);
                    ended.TrySetResult(reachedEnd);
                },
            });
            StartAutonomous(patrolState, "patrol");
            return ended.Task;
        }

        /// <summary>
        /// Holds the character for a scripted sequence that moves it itself (getting on a plaything): counts as
        /// autonomous behaviour, so the player's move input and timed idles are ignored, until
        /// <paramref name="done"/> returns true. Like a patrol it is replaced by a new patrol, follow or hold. The task
        /// completes when it leaves the stack: true when <paramref name="done"/> ended it, false when it was stopped
        /// first (replaced, popped, unwound), so the sequence can put the character back.
        /// </summary>
        public Task<bool> Hold(Func<bool> done)
        {
            var ended = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            bool finished = false;
            BaseState holdState = null;
            holdState = new BaseState(new()
            {
                ExitOnNextUpdate = () => finished = done(),
                OnExit = () =>
                {
                    AutonomousEnded(holdState);
                    ended.TrySetResult(finished);
                    return Task.CompletedTask;
                },
            });
            StartAutonomous(holdState, "hold");
            return ended.Task;
        }

        // replaces the running patrol / follow: its exit is requested first, so it exits (and puts its settings back)
        // before the new one is entered
        void StartAutonomous(BaseState state, string what)
        {
            if (_autonomous != null)
            {
                GD.Print($"{GetParent()?.Name}: a new {what} replaces the running patrol/follow");
                _ = Pop(_autonomous);
            }
            _autonomous = state;
            Push(state);
        }

        void AutonomousEnded(BaseState state)
        {
            if (_autonomous == state)
                _autonomous = null;
        }

        T Registered<T>() where T : BaseCharacterState =>
            _registeredStates.OfType<T>().FirstOrDefault()
            ?? throw new Exception($"CharacterStateManager requires a {typeof(T).Name} child node");
    }
}
