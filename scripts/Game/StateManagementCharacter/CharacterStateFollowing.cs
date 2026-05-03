using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.CharacterState
{
    [GlobalClass]
    public partial class CharacterStateFollowing : BaseCharacterState, IStateObject<CharacterStateFollowing.FollowOptions>
    {
        public struct FollowOptions
        {
            public Node3D Target;
            public CharacterController3D cc;
            public NavigationAgent3D agent;
        }

        FollowOptions _options;
        float _previousTargetDesiredDistance;
        float _previousSpeed;

        public BaseState GetState(FollowOptions options = default)
        {
            _options = options;
            _previousSpeed = options.cc.Speed;
            _previousTargetDesiredDistance = _options.agent.TargetDesiredDistance;
            _options.agent.TargetDesiredDistance = .5f;

            if (options.Target is Character3D)
                options.cc.Speed = (options.Target as Character3D).FindAnyObjectByType<CharacterController3D>().Speed;

            return new BaseState(new() { OnUpdate = OnUpdate, OnExit = OnExit });
        }

        private Task OnExit()
        {
            _options.cc.Speed = _previousSpeed;
            _options.agent.TargetDesiredDistance = _previousTargetDesiredDistance;
            return Task.CompletedTask;
        }

        private async void OnUpdate()
        {
            if (_options.Target == null)
                return;

            if (!_options.agent.IsNavigationFinished())
            {
                Vector3 _direction = _options.agent.GetNextPathPosition() - _options.cc.GlobalPosition;
                _options.cc.Move(_direction.ToVector2XZ());
            }

            await FindPath();
        }

        async Task FindPath()
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

            _options.agent.TargetPosition = _options.Target.GlobalPosition;
        }
    }
}
