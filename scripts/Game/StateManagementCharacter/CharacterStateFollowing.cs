using System;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.State;

namespace TnT.EduGame.Characters.States
{
    /// <summary>
    /// Follows a target with the character's <see cref="NavigationAgent3D"/>, at the target character's speed. Every
    /// push gets its own <see cref="Session"/>: the speed and target distance it changes are saved when it is entered
    /// and put back when it exits (finding 4.7: a second follow used to save the first one's speed as the original).
    /// </summary>
    [GlobalClass]
    public partial class CharacterStateFollowing : BaseCharacterState, IStateObject<CharacterStateFollowing.FollowOptions>
    {
        public struct FollowOptions
        {
            public Node3D Target;
            public CharacterController3D cc;
            public NavigationAgent3D agent;
            /// <summary>Called once, when the state leaves the stack.</summary>
            public Action ended;
        }

        public BaseState GetState(FollowOptions options = default)
        {
            var session = new Session(this, options);
            return new BaseState(new()
            {
                OnEnter = session.Enter,
                OnPause = session.Pause,
                OnResume = session.Resume,
                OnExit = session.Exit,
                OnUpdate = session.Update,
            });
        }

        sealed class Session
        {
            readonly CharacterStateFollowing _node;
            readonly FollowOptions _options;
            bool _entered;
            bool _active;   // the active top: only then may a path update (also one awaiting a physics frame) steer
            float _previousTargetDesiredDistance;
            float _previousSpeed;

            public Session(CharacterStateFollowing node, FollowOptions options)
            {
                _node = node;
                _options = options;
            }

            public Task Enter()
            {
                _entered = true;
                _previousSpeed = _options.cc.Speed;
                _previousTargetDesiredDistance = _options.agent.TargetDesiredDistance;
                _options.agent.TargetDesiredDistance = .5f;

                if (_options.Target is Character3D target)
                    _options.cc.Speed = target.FindAnyObjectByType<CharacterController3D>(includeSelf: true).Speed;
                _active = true;
                return Task.CompletedTask;
            }

            public Task Pause()
            {
                _active = false;
                return Task.CompletedTask;
            }

            public Task Resume()
            {
                _active = true;
                return Task.CompletedTask;
            }

            public Task Exit()
            {
                _active = false;
                if (_entered)
                {
                    _options.cc.Speed = _previousSpeed;
                    _options.agent.TargetDesiredDistance = _previousTargetDesiredDistance;
                }
                _options.ended?.Invoke();
                return Task.CompletedTask;
            }

            // the nodes this follow drives are still there: the target can be freed on its own, and after a scene
            // change a path update awaiting a physics frame may resume after them
            bool NodesValid =>
                GodotObject.IsInstanceValid(_node) && _node.IsInsideTree()
                && GodotObject.IsInstanceValid(_options.Target) && _options.Target.IsInsideTree()
                && GodotObject.IsInstanceValid(_options.agent) && GodotObject.IsInstanceValid(_options.cc);

            public void Update()
            {
                if (!_active || !NodesValid)
                    return;

                if (!_options.agent.IsNavigationFinished())
                {
                    Vector3 _direction = _options.agent.GetNextPathPosition() - _options.cc.GlobalPosition;
                    _options.cc.MoveInWorld(_direction);   // world space: right also for a rotated body
                }

                FindPath();
            }

            // async void: started from Update (a plain callback); nothing awaits it, so an exception is logged here
            async void FindPath()
            {
                try
                {
                    await _node.ToSignal(_node.GetTree(), SceneTree.SignalName.PhysicsFrame);

                    // this follow may have been replaced or paused meanwhile: don't steer the agent of the state
                    // after it; or its nodes are gone (scene change, target freed)
                    if (!_active || !NodesValid)
                        return;
                    _options.agent.TargetPosition = _options.Target.GlobalPosition;
                }
                catch (Exception e)
                {
                    GD.PushError($"CharacterStateFollowing: path update failed: {e}");
                }
            }
        }
    }
}
