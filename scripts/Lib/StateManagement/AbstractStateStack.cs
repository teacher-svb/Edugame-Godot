using System.Collections.Generic;
using System.Threading.Tasks;
namespace TnT.Systems.State
{

    public abstract partial class AbstractStateStack : AbstractStateManager
    {
        Stack<BaseState> _states = new Stack<BaseState>();
        private BaseState currentState = null;
        protected sealed override BaseState CurrentState { get => currentState; set => currentState = value; }
        protected sealed override BaseState NextState
        {
            get
            {
                BaseState result;
                _states.TryPeek(out result);
                return result;
            }
        }
        public void Push(BaseState state)
        {
            _states.Push(state);
        }

        public async void Pop()
        {
            await TransitionOut();
        }

        protected async override Task TransitionIn()
        {
            Transitioning = true;
            CurrentState = NextState;
            await CurrentState.EnterState();
            Transitioning = false;
        }

        protected async override Task TransitionOut()
        {
            Transitioning = true;
            var state = _states.Pop();
            await state.ExitState();
            Transitioning = false;
        }
    }
}