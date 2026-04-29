using System;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStatePatrolling : BaseCharacterState, IStateObject<CharacterStatePatrolling.PatrolOptions>
    {
        public struct PatrolOptions
        {
            public Node3D[] targets;
            public CharacterController3D cc;
            public NavigationAgent3D agent;
        }
        [Export]
        bool _loop = true;
        int _currentIndex = 0;

        PatrolOptions _options;

        public BaseState GetState(PatrolOptions options = default)
        {
            _options = options;
            return new BaseState(new() { OnEnter = OnEnter, OnExit = OnExit, OnUpdate = OnUpdate, ExitOnNextUpdate = IsDone });
        }

        private bool IsDone()
        {
            return _loop == false && _options.agent.IsTargetReached();
        }

        private void OnUpdate()
        {
            if (_currentIndex >= _options.targets.Length)
                return;

            var nextPos = _options.agent.GetNextPathPosition();
            Vector3 direction = nextPos - _options.cc.GlobalPosition;

            _options.cc.Move(direction.ToVector2XZ());
        }

        Task OnEnter()
        {
            GD.Print("entering patrol state");
            _options.agent.TargetReached += FindPath;

            if (_currentIndex < _options.targets.Length)
                _options.agent.TargetPosition = _options.targets[_currentIndex].GlobalPosition;
            
            return Task.CompletedTask;
        }

        Task OnExit()
        {
            GD.Print("exiting patrol state");
            _options.agent.TargetReached -= FindPath;
            return Task.CompletedTask;
        }

        async void FindPath()
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

            _currentIndex++;

            if (_currentIndex >= _options.targets.Length && _loop)
                _currentIndex = 0;

            if (_currentIndex >= _options.targets.Length)
                return;

            _options.agent.TargetPosition = _options.targets[_currentIndex].GlobalPosition;
        }
    }
}
