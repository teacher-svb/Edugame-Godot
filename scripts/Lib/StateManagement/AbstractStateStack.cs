using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.State
{
    /// <summary>
    /// A stack of <see cref="BaseState"/>s; only the top one is active. Each frame, when no transition runs, the stack
    /// does one step: it exits a state whose exit was requested (<see cref="Pop(BaseState)"/>, or its
    /// <c>ExitOnNextUpdate</c>), activates a new top (pausing the state it covers, then entering it or, if it was
    /// entered before, resuming it), or updates the active top. One transition at a time: pushes and pop requests made
    /// meanwhile are handled once it is done. An exception in a state's hook is logged and does not stop the stack.
    /// The bottom state is never popped.
    /// </summary>
    public abstract partial class AbstractStateStack : Node
    {
        private readonly List<BaseState> _states = new();   // bottom first
        private readonly Dictionary<BaseState, TaskCompletionSource> _exitRequests = new();
        private BaseState _currentState = null;   // the active top: entered or resumed, not covered or exited since
        private bool _transitioning = false;

        protected bool Transitioning => _transitioning;
        protected int StateCount => _states.Count;
        protected BaseState Top => _states.Count > 0 ? _states[^1] : null;
        protected BaseState Bottom => _states.Count > 0 ? _states[0] : null;
        /// <summary>The top state when it is active (entered or resumed) and no transition runs; otherwise null.</summary>
        protected BaseState ActiveState => !_transitioning && _currentState != null && _currentState == Top ? _currentState : null;
        protected bool Contains(BaseState state) => state != null && _states.Contains(state);

        public override async void _Process(double delta)
        {
            if (_transitioning)
                return;
            var top = Top;
            if (top == null)
                return;

            if (top == _currentState && !_exitRequests.ContainsKey(top))
            {
                bool exit;
                try { exit = top.UpdateState(); }
                catch (Exception e) { LogError("update", e); return; }
                if (exit)
                    _ = Pop();   // virtual: a subclass may hook in; the request is handled right below
            }

            var requested = TopmostExitRequest();
            if (requested != null)
                await ExitTransition(requested);
            else if (top != _currentState)
                await ActivateTransition(top);
        }

        public void Push(BaseState state)
        {
            if (!CanAdd(state))
                return;
            _states.Add(state);
        }

        /// <summary>Puts <paramref name="state"/> at <paramref name="index"/> (0 = bottom); clamped to the stack.</summary>
        protected void Insert(int index, BaseState state)
        {
            if (!CanAdd(state))
                return;
            _states.Insert(Math.Clamp(index, 0, _states.Count), state);
        }

        /// <summary>Puts <paramref name="state"/> directly below <paramref name="anchor"/> (on top when it isn't on the stack).</summary>
        protected void InsertBelow(BaseState anchor, BaseState state)
        {
            int index = _states.IndexOf(anchor);
            if (index < 0)
                Push(state);
            else
                Insert(index, state);
        }

        bool CanAdd(BaseState state)
        {
            if (state == null)
            {
                GD.PushError($"{Name}: cannot push a null state");
                return false;
            }
            if (_states.Contains(state))
            {
                GD.PushError($"{Name}: this state is already on the stack");
                return false;
            }
            return true;
        }

        /// <summary>Pops the state that is on top now (once no other transition runs). Never pops the bottom state.</summary>
        public virtual Task Pop()
        {
            if (StateCount <= 1)
                return Task.CompletedTask;
            return Pop(Top);
        }

        /// <summary>
        /// Removes <paramref name="state"/> from the stack once no other transition runs, wherever it is (a state
        /// below the top is exited in place, the ones above it stay). The task completes after its OnExit. Popping a
        /// state that is not on the stack does nothing; the bottom state is never popped.
        /// </summary>
        public Task Pop(BaseState state)
        {
            int index = _states.IndexOf(state);
            if (index < 0)
                return Task.CompletedTask;
            if (index == 0)
            {
                GD.PushWarning($"{Name}: the bottom state is never popped");
                return Task.CompletedTask;
            }
            if (!_exitRequests.TryGetValue(state, out var request))
            {
                request = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _exitRequests[state] = request;
            }
            return request.Task;
        }

        /// <summary>
        /// Exits every state between the bottom one and <paramref name="anchor"/> (both stay), the top one first, each
        /// removed before its OnExit runs. Only the states there when it is called: one pushed meanwhile stays. Meant
        /// to be called from a state's own hook (e.g. a loader unwinding the stack below itself), so inside a
        /// transition.
        /// </summary>
        protected async Task ExitStatesBelow(BaseState anchor)
        {
            int index = _states.IndexOf(anchor);
            if (index <= 1)
                return;
            var below = _states.GetRange(1, index - 1);
            bool own = !_transitioning;
            _transitioning = true;
            try
            {
                for (int i = below.Count - 1; i >= 0; i--)
                    await RemoveAndExit(below[i]);
            }
            finally
            {
                if (own)
                    _transitioning = false;
            }
        }

        BaseState TopmostExitRequest()
        {
            for (int i = _states.Count - 1; i >= 1; i--)
                if (_exitRequests.ContainsKey(_states[i]))
                    return _states[i];
            return null;
        }

        async Task ActivateTransition(BaseState top)
        {
            _transitioning = true;
            try
            {
                var covered = _currentState;
                _currentState = top;
                if (covered != null && covered != top && _states.Contains(covered))
                    await Run(covered.PauseState, "pause");

                if (top.IsEntered)
                    await Run(top.ResumeState, "resume");
                else
                {
                    top.IsEntered = true;
                    await Run(top.EnterState, "enter");
                }
            }
            finally
            {
                _transitioning = false;
            }
        }

        async Task ExitTransition(BaseState state)
        {
            _transitioning = true;
            try
            {
                await RemoveAndExit(state);
            }
            finally
            {
                _transitioning = false;
            }
        }

        // removed before its OnExit runs, so a state pushed from OnExit lands on the stack without it
        async Task RemoveAndExit(BaseState state)
        {
            _states.Remove(state);
            if (state == _currentState)
                _currentState = null;
            try
            {
                await Run(state.ExitState, "exit");
            }
            finally
            {
                if (_exitRequests.Remove(state, out var request))
                    request.TrySetResult();
            }
        }

        async Task Run(Func<Task> hook, string phase)
        {
            try
            {
                await hook();
            }
            catch (Exception e)
            {
                LogError(phase, e);
            }
        }

        // also after an await: a hook may fail because the stack's node was freed under it (scene change)
        void LogError(string phase, Exception e) =>
            GD.PushError($"{(IsInstanceValid(this) ? Name : GetType().Name)}: state {phase} failed: {e}");
    }
}
