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
    /// The follower keeps its distance: it stops at <see cref="_followDistance"/> and only walks again once the target
    /// is past <see cref="_resumeDistance"/>, and it eases its speed (slower close to the follow distance, a little
    /// faster than the target when it fell behind) instead of jumping between a full run and a stop.
    /// </summary>
    [GlobalClass]
    public partial class CharacterStateFollowing : BaseCharacterState, IStateObject<CharacterStateFollowing.FollowOptions>
    {
        /// <summary>Metres (horizontal) to the target where the follower stops.</summary>
        [Export] float _followDistance = 1.5f;
        /// <summary>Metres to the target where a stopped follower walks again; more than the follow distance, so it
        /// doesn't start and stop at every small step of the target.</summary>
        [Export] float _resumeDistance = 2.25f;
        /// <summary>Metres beyond the follow distance over which the speed goes from <see cref="_minSpeedFactor"/> to
        /// the target's speed (and on to <see cref="_maxSpeedFactor"/> when further behind).</summary>
        [Export] float _slowDownRange = 1.0f;
        [Export] float _minSpeedFactor = 0.35f;
        /// <summary>Speed factor when far behind, so the follower catches up with a target that keeps walking.</summary>
        [Export] float _maxSpeedFactor = 1.25f;
        /// <summary>m/s², speeding up and slowing down.</summary>
        [Export] float _acceleration = 8f;

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
            float _fullSpeed;       // the target's speed (or the follower's own, for a target that isn't a character)
            float _currentSpeed;    // eased toward the speed wanted for the distance
            bool _walking;          // past the resume distance and not yet back at the follow distance

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
                // small: the agent keeps steering while the follower slows down to a stop at the follow distance
                _options.agent.TargetDesiredDistance = .5f;

                _fullSpeed = _options.Target is Character3D target
                    ? target.FindAnyObjectByType<CharacterController3D>(includeSelf: true).Speed
                    : _options.cc.Speed;
                _currentSpeed = 0;
                _walking = false;
                _active = true;
                return Task.CompletedTask;
            }

            public Task Pause()
            {
                _active = false;
                _currentSpeed = 0;
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

                var offset = _options.Target.GlobalPosition - _options.cc.GlobalPosition;
                offset.Y = 0;
                float distance = offset.Length();
                if (_walking && distance <= _node._followDistance)
                    _walking = false;
                else if (!_walking && distance >= Mathf.Max(_node._resumeDistance, _node._followDistance))
                    _walking = true;

                float wanted = 0;
                if (_walking)
                {
                    float factor = (distance - _node._followDistance) / Mathf.Max(_node._slowDownRange, 0.01f);
                    wanted = _fullSpeed * Mathf.Clamp(factor, _node._minSpeedFactor, _node._maxSpeedFactor);
                }
                float delta = (float)_node.GetProcessDeltaTime();
                _currentSpeed = Mathf.MoveToward(_currentSpeed, wanted, _node._acceleration * delta);

                // below a crawl it stands (and plays idle) instead of running in place
                if (_currentSpeed > 0.05f && !_options.agent.IsNavigationFinished())
                {
                    _options.cc.Speed = _currentSpeed;
                    Vector3 _direction = _options.agent.GetNextPathPosition() - _options.cc.GlobalPosition;
                    _options.cc.MoveInWorld(_direction);   // world space: right also for a rotated body
                }
                else
                    _currentSpeed = Mathf.Min(_currentSpeed, 0.05f);

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
