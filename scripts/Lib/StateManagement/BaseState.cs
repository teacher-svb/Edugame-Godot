using System.Threading.Tasks;

namespace TnT.Systems.State
{
    /// <summary>
    /// One entry on an <see cref="AbstractStateStack"/>. Lifecycle, driven by the stack:
    /// <list type="bullet">
    /// <item><c>OnEnter</c> once, when it first becomes the top.</item>
    /// <item><c>OnPause</c> when another state is entered on top of it; <c>OnResume</c> when it is the top again
    /// after the states above it are gone (not a second <c>OnEnter</c>). A state that never became the top gets
    /// neither.</item>
    /// <item><c>OnUpdate</c> every frame while it is the active top; <c>ExitOnNextUpdate</c> returning true pops it.</item>
    /// <item><c>OnExit</c> once, when it is removed from the stack: also for a state that was never entered (pushed,
    /// then covered or unwound before its turn), so it can release what its <c>GetState</c> set up.</item>
    /// </list>
    /// </summary>
    public class BaseState
    {
        public delegate Task OnEnter();
        public delegate Task OnExit();
        public delegate Task OnPause();
        public delegate Task OnResume();
        public delegate void OnUpdate();
        public delegate bool Exit();
        public class StateOptions
        {
            public OnEnter OnEnter { get; set; }
            public OnExit OnExit { get; set; }
            /// <summary>Another state was entered on top of this one.</summary>
            public OnPause OnPause { get; set; }
            /// <summary>This state is the top again (the states above it were popped); it was entered before.</summary>
            public OnResume OnResume { get; set; }
            public OnUpdate OnUpdate { get; set; }
            public Exit ExitOnNextUpdate { get; set; } = () => false;
        }
        protected StateOptions Options { get; set; }

        /// <summary>True once <c>OnEnter</c> has run (set by the stack).</summary>
        public bool IsEntered { get; internal set; }

        public BaseState(StateOptions options)
        {
            Options = options;
        }

        public static BaseState GetEmptyState() => new BaseState(new() { });
        public async Task EnterState()
        {
            await (Options.OnEnter?.Invoke() ?? Task.CompletedTask);
        }
        public async Task ExitState()
        {
            await (Options.OnExit?.Invoke() ?? Task.CompletedTask);
        }
        public async Task PauseState()
        {
            await (Options.OnPause?.Invoke() ?? Task.CompletedTask);
        }
        public async Task ResumeState()
        {
            await (Options.OnResume?.Invoke() ?? Task.CompletedTask);
        }
        public bool UpdateState()
        {
            if (Options.ExitOnNextUpdate != null && Options.ExitOnNextUpdate.Invoke())
                return true;

            Options.OnUpdate?.Invoke();
            return false;
        }
    }
}
