using System.Threading.Tasks;

namespace TnT.Systems.State
{
    public class BaseState
    {
        public delegate Task OnEnter();
        public delegate Task OnExit();
        public delegate void OnUpdate();
        public delegate bool Exit();
        public class StateOptions
        {
            public OnEnter OnEnter { get; set; }
            public OnExit OnExit { get; set; }
            public OnUpdate OnUpdate { get; set; }
            public Exit ExitOnNextUpdate { get; set; } = () => false;
        }
        protected StateOptions Options { get; set; }

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
        public bool UpdateState()
        {
            if (Options.ExitOnNextUpdate != null && Options.ExitOnNextUpdate.Invoke())
                return true;

            Options.OnUpdate?.Invoke();
            return false;
        }
    }
}