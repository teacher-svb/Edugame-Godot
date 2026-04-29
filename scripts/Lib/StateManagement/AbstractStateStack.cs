using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.State
{
    public abstract partial class AbstractStateStack : Node
    {
        private readonly Stack<BaseState> _states = new();
        private BaseState _currentState = null;
        private bool _transitioning = false;

        protected bool Transitioning { get => _transitioning; set => _transitioning = value; }
        protected int StateCount => _states.Count;

        private BaseState NextState
        {
            get
            {
                _states.TryPeek(out var result);
                return result;
            }
        }

        public async override void _Process(double delta)
        {
            if (_transitioning || NextState == null)
                return;

            if (NextState.Equals(_currentState))
            {
                if (_currentState.UpdateState())
                    await Pop();
            }
            else
                await TransitionIn();
        }

        public void Push(BaseState state)
        {
            _states.Push(state);
        }

        public virtual async Task Pop()
        {
            if (StateCount <= 1) return;
            await TransitionOut();
        }

        protected async Task TransitionIn()
        {
            _transitioning = true;
            _currentState = NextState;
            await _currentState.EnterState();
            _transitioning = false;
        }

        protected async Task TransitionOut()
        {
            _transitioning = true;
            var state = _states.Pop();
            await state.ExitState();
            _transitioning = false;
        }
    }
}
