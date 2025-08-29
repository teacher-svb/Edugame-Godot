using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.State
{
    public abstract partial class AbstractStateManager : Node
    {
        protected abstract BaseState CurrentState { get; set; }
        protected abstract BaseState NextState { get; }

        private bool _transitioning = false;

        protected bool Transitioning { get => _transitioning; set => _transitioning = value; }

        // public virtual void _Ready() { }

        public async override void _Process(double delta)
        {
            if (Transitioning || NextState == null)
                return;

            if (NextState.Equals(CurrentState))
            {
                if (CurrentState.UpdateState())
                    await TransitionOut();
            }
            else
                await TransitionIn();
        }

        protected abstract Task TransitionIn();
        protected abstract Task TransitionOut();
    }
}