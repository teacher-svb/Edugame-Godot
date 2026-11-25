using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.State;

namespace TnT.EduGame.Characters.States
{
    /// <summary>
    /// Walks a character along waypoints with its <see cref="NavigationAgent3D"/>. Every push gets its own
    /// <see cref="Session"/> (route, index, stuck timer), so two patrols never share progress (finding 4.7).
    /// A waypoint counts as done when the agent reaches it, and also, with a warning, when the agent can't get there:
    /// its path ends short of it (off the navmesh, across a gap) or the character hardly moves for
    /// <see cref="StuckSeconds"/> (blocked). So a non-looping patrol always ends (finding 4.6).
    /// </summary>
    [GlobalClass]
    public partial class CharacterStatePatrolling : BaseCharacterState, IStateObject<CharacterStatePatrolling.PatrolOptions>
    {
        public struct PatrolOptions
        {
            public Node3D[] targets;
            public CharacterController3D cc;
            public NavigationAgent3D agent;
            /// <summary>Called once, when the state leaves the stack: true when the route was walked to its end,
            /// false when it was stopped first (replaced by another patrol or follow, popped, unwound).</summary>
            public Action<bool> ended;
        }

        /// <summary>Seconds without moving <see cref="StuckDistance"/> before a waypoint is given up.</summary>
        public const double StuckSeconds = 4.0;
        public const float StuckDistance = 0.25f;

        [Export]
        bool _loop = true;

        public BaseState GetState(PatrolOptions options = default)
        {
            var session = new Session(this, options, _loop);
            return new BaseState(new()
            {
                OnEnter = session.Enter,
                OnPause = session.Pause,
                OnResume = session.Resume,
                OnExit = session.Exit,
                OnUpdate = session.Update,
                ExitOnNextUpdate = () => session.Done,
            });
        }

        sealed class Session
        {
            readonly CharacterStatePatrolling _node;
            readonly PatrolOptions _options;
            readonly Node3D[] _targets;
            readonly bool _loop;
            readonly bool[] _warned;   // one warning per waypoint, not one per lap of a looping patrol
            int _index;
            bool _entered;
            bool _reachedEnd;
            Vector3 _stuckAnchor;
            double _stuckTime;

            public Session(CharacterStatePatrolling node, PatrolOptions options, bool loop)
            {
                _node = node;
                _options = options;
                _targets = options.targets ?? [];
                _loop = loop;
                _warned = new bool[_targets.Length];
                if (_targets.Length == 0)
                {
                    GD.PushWarning($"{CharacterName}: patrol without waypoints, ends at once");
                    _reachedEnd = true;
                }
            }

            public bool Done => _reachedEnd;

            string CharacterName => _options.cc?.GetParent()?.Name ?? _node.Name;

            public Task Enter()
            {
                _entered = true;
                _index = 0;
                HeadForCurrent();
                return Task.CompletedTask;
            }

            public Task Pause() => Task.CompletedTask;

            // a state on top of it was popped: head for the current waypoint again (not the route from the start)
            public Task Resume()
            {
                HeadForCurrent();
                return Task.CompletedTask;
            }

            public Task Exit()
            {
                // also for a session that never ran (replaced before its turn): its awaiter must not hang
                _options.ended?.Invoke(_entered && _reachedEnd);
                return Task.CompletedTask;
            }

            public void Update()
            {
                if (_reachedEnd)
                    return;

                var agent = _options.agent;
                var cc = _options.cc;
                // drives the agent: updates its path and its reached / finished flags
                var nextPos = agent.GetNextPathPosition();

                if (agent.IsTargetReached())
                {
                    Advance();
                    return;
                }
                if (agent.IsNavigationFinished())
                {
                    Warn("can't reach", "the path ends");
                    Advance();
                    return;
                }

                _stuckTime += _node.GetProcessDeltaTime();
                if (cc.GlobalPosition.DistanceTo(_stuckAnchor) >= StuckDistance)
                    ResetStuck();
                else if (_stuckTime >= StuckSeconds)
                {
                    Warn("is stuck on the way to", "it stands");
                    Advance();
                    return;
                }

                cc.MoveInWorld(nextPos - cc.GlobalPosition);   // world space: right also for a rotated body
            }

            void Warn(string what, string where)
            {
                if (_warned[_index])
                    return;
                _warned[_index] = true;
                var target = _targets[_index];
                float distance = _options.cc.GlobalPosition.DistanceTo(target.GlobalPosition);
                GD.PushWarning($"{CharacterName}: patrol {what} waypoint '{target.Name}' ({where} {distance:0.0} m from it), "
                    + "skipping it");
            }

            void Advance()
            {
                _index++;
                if (_index >= _targets.Length)
                {
                    if (!_loop)
                    {
                        _reachedEnd = true;
                        return;
                    }
                    _index = 0;
                }
                HeadForCurrent();
            }

            void HeadForCurrent()
            {
                if (_reachedEnd || _index >= _targets.Length)
                    return;
                _options.agent.TargetPosition = _targets[_index].GlobalPosition;
                ResetStuck();
            }

            void ResetStuck()
            {
                _stuckAnchor = _options.cc.GlobalPosition;
                _stuckTime = 0;
            }
        }
    }
}
