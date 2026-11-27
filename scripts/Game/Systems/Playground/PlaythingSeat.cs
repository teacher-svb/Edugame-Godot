using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.Characters.States;
using TnT.EduGame.GameStates;
using TnT.EduGame.Interaction;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// A seat on a plaything (a swing seat, a seesaw end) the player gets on with the interact key.
    /// With a <see cref="Swing"/>: while seated, the jump key pumps the swing (a second key label shows it), and E
    /// while the swing is high enough waits for the next forward swing and jumps off forward, landing on the ground in
    /// front; low, E gets off as without a swing. With a <see cref="Disc"/> (merry-go-round): the jump key pushes the
    /// disc round, and E jumps off outward when it turns fast enough, else steps off outward. While the player is
    /// in this area, a key label ("E", <c>quest_prompt.tscn</c>) floats above <see cref="Seat"/>; E starts the sequence:
    /// the player walks to <see cref="Enter"/> (its navigation agent, then straight to the spot), turns to face the way
    /// <see cref="Enter"/>'s +Z points (away from the seat), jumps backwards onto the seat and sits down. E again: stands
    /// up and steps back to <see cref="Enter"/>, and the player has control again.
    /// </summary>
    /// <remarks>
    /// During the sequence the player is held (<see cref="CharacterStateManager.Hold"/>: no move input), the body is
    /// placed by this node (<see cref="CharacterController3D.MovementEnabled"/> off from the jump to the step back) and
    /// the animation states are set with <see cref="CharacterAnimator.Hold"/>: "jump", "fall", "sit_down" (which goes on
    /// to "sit" in the AnimationTree), "stand_up" (then "idle"). One seat at a time: while the player uses one, the others
    /// don't react. A hold replaced from elsewhere (a quest walking the player) puts the player back at
    /// <see cref="Enter"/>. The seat doesn't move yet (the plaything's own animation isn't played).
    /// </remarks>
    [GlobalClass]
    public partial class PlaythingSeat : Area3D, IInteractable
    {
        const string DefaultPromptScene = "res://Scenes/interactables/quest_prompt.tscn";

        /// <summary>Where the player stands to get on and off (on the ground); +Z is the way they face then.</summary>
        [Export] public Node3D Enter { get; set; }
        /// <summary>The seat's marker (top of the seat), moving with the seat; +Z is the way the sitting player faces.</summary>
        [Export] public Node3D Seat { get; set; }
        /// <summary>The player's origin (feet) while sitting, in <see cref="Seat"/>'s space: the "sit" animation puts the
        /// seat contact 0.235 m above the origin and 0.21 m behind it (KayKit Sit_Chair at scale 0.525).</summary>
        [Export] public Vector3 SitOffset { get; set; } = new(0, -0.235f, 0.21f);
        [Export] public float JumpHeight { get; set; } = 0.25f;
        [Export] public float JumpSeconds { get; set; } = 0.45f;
        /// <summary>Height of the prompt above <see cref="Seat"/>.</summary>
        [Export] public float PromptHeight { get; set; } = 1.5f;
        /// <summary>Where the E shows while the player sits (it gets them off), in <see cref="Enter"/>'s space: on the
        /// ground in front, past the seat's forward swing (a swing seat reaches 0.9 m forward), so the rider doesn't
        /// swing through it.</summary>
        [Export] public Vector3 GetOffPromptOffset { get; set; } = new(0, 0.25f, 1.0f);
        /// <summary>Shown while the key works here; empty: an instance of <c>quest_prompt.tscn</c>.</summary>
        [Export] public Node3D Prompt { get; set; }
        /// <summary>The pendulum that swings this seat; empty: the seat doesn't move (seesaw end, bench).</summary>
        [Export] public SwingPendulum Swing { get; set; }
        /// <summary>Below this swing height (degrees) E gets off as on a still seat; above it, the player jumps off.</summary>
        [Export] public float MinJumpOffAngle { get; set; } = 8f;
        /// <summary>Jumping off: the player leaves the seat on its way forward at this angle (degrees, or 60% of the
        /// swing's height when that is lower), with the seat's speed plus <see cref="JumpOffBoost"/>.</summary>
        [Export] public float JumpOffAngle { get; set; } = 15f;
        /// <summary>Added to the seat's speed when jumping off: up, and forward along <see cref="Enter"/>'s +Z (m/s).</summary>
        [Export] public Vector2 JumpOffBoost { get; set; } = new(0.5f, 1.2f);
        /// <summary>The disc this seat turns with (merry-go-round); empty: none. The jump key pushes it; getting off
        /// goes outward, away from its centre.</summary>
        [Export] public SpinningDisc Disc { get; set; }
        /// <summary>With a <see cref="Disc"/>: at this rider speed (m/s) or more E jumps off, below it the rider steps off.</summary>
        [Export] public float MinJumpOffSpeed { get; set; } = 0.6f;
        /// <summary>With a <see cref="Disc"/>: stepping off lands this far from its centre (m).</summary>
        [Export] public float StepOffRadius { get; set; } = 1.15f;
        /// <summary>The seesaw beam this seat rocks with; empty: none. The jump key bounces it (one bounce per press,
        /// from rest); E while it bounces gets off once it is back at rest.</summary>
        [Export] public SeesawBeam Beam { get; set; }
        /// <summary>Optional <see cref="IPlaythingPartner"/> (the seesaw's <see cref="AvocadoSpot"/>): the player only
        /// gets on while it <see cref="IPlaythingPartner.IsReady"/> (else it explains why), and it is told when the
        /// player got off.</summary>
        [Export] public Node Partner { get; set; }
        IPlaythingPartner PartnerOrNull => Partner as IPlaythingPartner;
        /// <summary>The plaything looks the same after a 1/N turn (a merry-go-round with 4 bars: 4): getting on, the
        /// player takes the copy of <see cref="Seat"/> turned by k/N about its parent's Y axis that is nearest to
        /// <see cref="Enter"/>, so the disc may have stopped at any angle. 1: always <see cref="Seat"/> as placed.</summary>
        [Export(PropertyHint.Range, "1,12")] public int SeatSymmetry { get; set; } = 1;
        /// <summary>Animation state after landing on the seat ("sit_down", which goes on to "sit"; "grab_onto" standing).</summary>
        [Export] public string SettleState { get; set; } = "sit_down";
        /// <summary>Seconds in <see cref="SettleState"/> before the player is seated (and E works); negative: its clip's length.</summary>
        [Export] public float SettleSeconds { get; set; } = -1f;
        /// <summary>Animation state played before stepping off ("stand_up"); empty: step off at once.</summary>
        [Export] public string GetUpState { get; set; } = "stand_up";

        enum Phase { Free, Approaching, Catching, Jumping, SittingDown, Seated, StandingUp, SteppingOff, Flying }

        // the seat the player is on or getting on (one at a time)
        static PlaythingSeat _inUse;

        const float CatchAngle = 2f;          // degrees: a braked swing this low is stopped and the player hops on

        InputAction InteractAction => StateManagerGame.Instance?.PickupItemAction;
        InputAction PumpAction => StateManagerGame.Instance?.JumpPlayerAction;
        const float Gravity = 9.8f;
        bool _playerInRange;
        Phase _phase = Phase.Free;
        double _timer;
        bool _holdDone;
        Tween _tween;
        Label3D _pumpLabel;

        // the model (VisualRoot) follows the seat while seated; the body stays where it landed
        Node3D _model;
        Transform3D _modelRest;
        bool _modelAttached;
        Vector3 _seatPrev;
        Vector3 _seatVelocity;
        bool _jumpOffQueued;
        Vector3 _flyVelocity;

        Transform3D _seatBase;   // Seat's local transform as placed, for SeatSymmetry

        Character3D _body;
        CharacterController3D _cc;
        CharacterStateManager _states;
        CharacterAnimator _animator;
        NavigationAgent3D _agent;
        PlayerApproach _approach;
        bool _getOffQueued;   // a seesaw: E while bouncing gets off once the player's end is down again

        public override void _Ready()
        {
            if (Enter == null || Seat == null)
                GD.PrintErr($"PlaythingSeat '{Name}': Enter or Seat not set.");
            else
                _seatBase = Seat.Transform;

            if (Prompt == null && GD.Load<PackedScene>(DefaultPromptScene) is PackedScene scene)
            {
                Prompt = scene.Instantiate<Node3D>();
                AddChild(Prompt);
            }
            if (Prompt != null)
            {
                Prompt.TopLevel = true;
                var label = Prompt.FindAnyObjectByType<Label3D>(includeSelf: true);
                if (label != null && InteractionFocus.KeyLabel(InteractAction) is string key)
                    label.Text = key;
                // the pump key, under the E while seated on a swing
                if ((Swing != null || Disc != null || Beam != null) && label != null)
                {
                    _pumpLabel = (Label3D)label.Duplicate();
                    _pumpLabel.Text = InteractionFocus.KeyLabel(PumpAction) ?? "Space";
                    _pumpLabel.FontSize = label.FontSize * 5 / 8;
                    _pumpLabel.TopLevel = true;   // stays above the seat while the E moves to the ground
                    _pumpLabel.Visible = false;
                    Prompt.AddChild(_pumpLabel);
                }
            }

            BodyEntered += b => { if (Player.IsPlayer(b)) { _playerInRange = true; InteractionFocus.Add(this); } };
            BodyExited += b => { if (Player.IsPlayer(b)) { _playerInRange = false; InteractionFocus.Remove(this); } };
            UpdatePrompt();
        }

        public override void _ExitTree()
        {
            _playerInRange = false;
            InteractionFocus.Remove(this);
            if (_phase != Phase.Free)
                PutBack();
        }

        public override void _Process(double delta)
        {
            UpdatePrompt();
            if (InteractionFocus.TryUse(this, InteractAction))
                Interact();
            // read in _Process: Triggered is true for one frame, which a physics frame can miss or see twice
            if (_phase == Phase.Seated && !_jumpOffQueued && PumpAction?.Triggered == true)
            {
                if (Swing != null)
                    Swing.Pump();
                else if (Disc != null)
                    Disc.Push();
                else
                    Beam?.Push();
            }
        }

        Vector3 IInteractable.InteractPosition =>
            _phase == Phase.Seated && IsInstanceValid(_body) ? _body.GlobalPosition : Enter?.GlobalPosition ?? GlobalPosition;

        bool IInteractable.CanInteract => _playerInRange && Enter != null && Seat != null
            && (_phase == Phase.Free ? _inUse == null && PartnerOrNull?.IsBusy != true : _phase == Phase.Seated);

        /// <summary>Whether the player sits on this seat now.</summary>
        public bool IsSeated => _phase == Phase.Seated;
        /// <summary>Whether the player is getting on, on or getting off this seat.</summary>
        public bool IsBusy => _phase != Phase.Free;

        /// <summary>Gets on (free) or off (seated), as if the player pressed the interact key.</summary>
        public void Interact()
        {
            if (_phase == Phase.Free && _inUse == null && PartnerOrNull?.IsBusy != true)
            {
                if (PartnerOrNull is IPlaythingPartner partner && !partner.IsReady)
                    partner.ExplainNotReady();
                else
                    Begin();
            }
            else if (_phase == Phase.Seated && Beam != null && !Beam.AtRest)
                _getOffQueued = true;
            else if (_phase == Phase.Seated && !_jumpOffQueued && !_getOffQueued)
            {
                if (Swing != null && Mathf.RadToDeg(Swing.Amplitude) >= MinJumpOffAngle)
                    _jumpOffQueued = true;   // leaves on the next forward swing (_PhysicsProcess)
                else if (Disc != null && Disc.SpeedAt(Seat.GlobalPosition) >= MinJumpOffSpeed)
                    StartJumpOff();
                else
                    StartStandingUp();
            }
        }

        void Begin()
        {
            _body = Player.Instance?.FindAncestorOfType<Character3D>();
            _cc = _body?.FindAnyObjectByType<CharacterController3D>(includeSelf: true);
            _states = _body?.FindAnyObjectByType<CharacterStateManager>(includeSelf: true);
            _animator = _body?.FindAnyObjectByType<CharacterAnimator>(includeSelf: true);
            _agent = _body?.FindAnyObjectByType<NavigationAgent3D>(includeSelf: true);
            if (_cc == null || _states == null || _animator == null)
            {
                GD.PrintErr($"PlaythingSeat '{Name}': the player has no controller, state manager or animator.");
                return;
            }

            _inUse = this;
            _holdDone = false;
            _jumpOffQueued = false;
            _getOffQueued = false;
            _model = _cc.VisualRoot;
            Swing?.Brake();   // a still-swinging seat is caught first; the hop waits until it hangs nearly still
            HoldPlayer();
            _approach = new PlayerApproach(_body, _cc, _agent);
            _approach.Start(Enter.GlobalPosition, Enter.GlobalBasis.Z);
            Switch(Phase.Approaching);
        }

        async void HoldPlayer()
        {
            try
            {
                bool ended = await _states.Hold(() => _holdDone);
                // replaced from elsewhere (a quest walking the player) while the sequence ran: put the player back
                if (!ended && IsInstanceValid(this) && _phase != Phase.Free)
                    PutBack();
            }
            catch (System.Exception e)
            {
                GD.PushError($"PlaythingSeat '{Name}': {e}");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_phase == Phase.Free)
                return;
            if (!IsInstanceValid(_body))
            {
                Finish();
                return;
            }
            _timer += delta;

            if (_modelAttached && IsInstanceValid(Seat))
            {
                var seatPos = Seat.GlobalPosition;
                _seatVelocity = (seatPos - _seatPrev) / (float)delta;
                _seatPrev = seatPos;
                FollowSeat();
            }

            switch (_phase)
            {
                case Phase.Approaching:
                    if (_approach.Step(delta))
                        Switch(Phase.Catching);
                    break;

                case Phase.Catching:   // a swing caught on the way: wait until it hangs nearly still
                    if (Swing == null || Mathf.RadToDeg(Swing.Amplitude) <= CatchAngle)
                        StartJump();
                    break;

                case Phase.SittingDown:
                    if (_timer >= (SettleSeconds >= 0 ? SettleSeconds : _animator.AnimationLength(SettleState)))
                        Switch(Phase.Seated);
                    break;

                case Phase.Seated:
                    if (_getOffQueued && (Beam == null || Beam.AtRest))
                    {
                        _getOffQueued = false;
                        StartStandingUp();
                    }
                    else if (_jumpOffQueued && Swing != null)
                    {
                        float release = Mathf.Min(Mathf.DegToRad(JumpOffAngle), Swing.Amplitude * 0.6f);
                        if (Swing.AngularVelocity > 0 && Swing.Angle >= release)
                            StartJumpOff();
                    }
                    break;

                case Phase.Flying:
                    _flyVelocity.Y -= Gravity * (float)delta;
                    var p = _body.GlobalPosition + _flyVelocity * (float)delta;
                    if (_flyVelocity.Y < 0)
                        _animator.Hold("fall");
                    float ground = Enter.GlobalPosition.Y;
                    if (_flyVelocity.Y < 0 && p.Y <= ground)
                    {
                        _body.GlobalPosition = new Vector3(p.X, ground, p.Z);
                        Finish(gotOff: true);
                        _animator.PlayLanding();   // the squash and the land clip, then back to idle
                    }
                    else
                        _body.GlobalPosition = p;
                    break;

                case Phase.StandingUp:
                    if (_timer >= _animator.AnimationLength(GetUpState))
                        StartSteppingOff();
                    break;
            }
        }

        // the turned copy of the seat (SeatSymmetry) nearest to Enter; the marker is moved there, under its parent
        void PickNearestSeat()
        {
            if (SeatSymmetry <= 1 || Seat == null)
                return;
            Transform3D best = _seatBase;
            float bestDistance = float.MaxValue;
            for (int k = 0; k < SeatSymmetry; k++)
            {
                var turned = new Transform3D(new Basis(Vector3.Up, Mathf.Tau * k / SeatSymmetry), Vector3.Zero) * _seatBase;
                Seat.Transform = turned;
                float d = Seat.GlobalPosition.DistanceSquaredTo(Enter.GlobalPosition);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = turned;
                }
            }
            Seat.Transform = best;
        }

        void StartJump()
        {
            PickNearestSeat();
            Swing?.Stop();
            Switch(Phase.Jumping);
            _cc.MovementEnabled = false;
            _animator.Hold("jump");
            var from = _body.GlobalPosition;
            // turn in the air to the way the player faces on the seat (a swing: the same way already)
            _cc.FaceTowards(_body.GlobalPosition + Flat(Seat.GlobalBasis.Z), JumpSeconds);
            Arc(from, () => Seat.GlobalTransform * SitOffset, JumpSeconds, JumpHeight, t =>
            {
                if (t >= 0.5f)
                    _animator.Hold("fall");
            }, () =>
            {
                _animator.Hold(SettleState);
                AttachModel();
                Switch(Phase.SittingDown);
            });
        }

        // from now on the model moves and tilts with the seat; the body (and so the camera) stays put
        void AttachModel()
        {
            if (!IsInstanceValid(_model))
                return;
            _modelRest = _model.Transform;
            _seatPrev = Seat.GlobalPosition;
            _seatVelocity = Vector3.Zero;
            _modelAttached = true;
        }

        void FollowSeat()
        {
            if (!IsInstanceValid(_model))
                return;
            var basis = Seat.GlobalBasis.Orthonormalized().Scaled(_modelRest.Basis.Scale);
            _model.GlobalTransform = new Transform3D(basis, Seat.GlobalTransform * SitOffset);
        }

        // the body takes the model's place, the model is back at its rest under the body
        void DetachModel()
        {
            if (!_modelAttached)
                return;
            _modelAttached = false;
            if (!IsInstanceValid(_model))
                return;
            var at = _model.GlobalPosition;
            var facing = Flat(_model.GlobalBasis.Z);   // the heading on the seat (a disc has turned the rider)
            _model.Transform = _modelRest;
            if (IsInstanceValid(_body))
                _body.GlobalPosition = at;
            if (IsInstanceValid(_cc) && facing.LengthSquared() > 0.0001f)
                _cc.FaceTowards(_model.GlobalPosition + facing, 0f);   // upright, with that heading
        }

        void StartJumpOff()
        {
            _jumpOffQueued = false;
            _flyVelocity = _seatVelocity + OffDirection() * JumpOffBoost.X + Vector3.Up * JumpOffBoost.Y;
            DetachModel();
            _animator.Hold("jump");
            Switch(Phase.Flying);
        }

        // a swing: forward (Enter's +Z); a disc: away from its centre, from where the rider is now
        Vector3 OffDirection()
        {
            if (Disc?.Disc != null)
            {
                var from = _modelAttached && IsInstanceValid(_model) ? _model.GlobalPosition : _body.GlobalPosition;
                var away = Flat(from - Disc.Disc.GlobalPosition);
                if (away.LengthSquared() > 0.0001f)
                    return away.Normalized();
            }
            return Flat(Enter.GlobalBasis.Z).Normalized();
        }

        void StartStandingUp()
        {
            if (string.IsNullOrEmpty(GetUpState))
            {
                StartSteppingOff();
                return;
            }
            _animator.Hold(GetUpState);
            Switch(Phase.StandingUp);
        }

        void StartSteppingOff()
        {
            // a disc: out to the ground beside where the rider is now (the disc may have turned them anywhere)
            var target = Enter.GlobalPosition;
            if (Disc?.Disc != null)
            {
                var c = Disc.Disc.GlobalPosition;
                target = new Vector3(c.X, Enter.GlobalPosition.Y, c.Z) + OffDirection() * StepOffRadius;
            }
            DetachModel();
            Switch(Phase.SteppingOff);
            _animator.Hold("idle");
            Arc(_body.GlobalPosition, () => target, Disc != null ? 0.4f : 0.3f, Disc != null ? 0.2f : 0.1f, null, () => Finish(gotOff: true));
        }

        // moves the body from `from` to `to()` (re-read every frame: the seat may move) on an arc `height` high
        void Arc(Vector3 from, System.Func<Vector3> to, float seconds, float height, System.Action<float> step, System.Action done)
        {
            _tween?.Kill();
            _tween = CreateTween();
            _tween.TweenMethod(Callable.From<float>(t =>
            {
                if (!IsInstanceValid(_body))
                    return;
                _body.GlobalPosition = from.Lerp(to(), t) + Vector3.Up * (height * 4f * t * (1f - t));
                step?.Invoke(t);
            }), 0f, 1f, seconds);
            _tween.Finished += () => done?.Invoke();
        }

        // a hold replaced, or this node leaving the tree mid-sequence: the player stands at Enter again
        void PutBack()
        {
            DetachModel();
            if (IsInstanceValid(_body) && Enter != null && IsInstanceValid(Enter) && _phase >= Phase.Jumping)
                _body.GlobalPosition = Enter.GlobalPosition;
            Finish();
        }

        // gotOff: the player got off normally (stepped or jumped off), so the partner is told; not when put back
        void Finish(bool gotOff = false)
        {
            DetachModel();
            _jumpOffQueued = false;
            _tween?.Kill();
            _tween = null;
            if (IsInstanceValid(_cc))
                _cc.MovementEnabled = true;
            if (IsInstanceValid(_animator))
                _animator.Release();
            _holdDone = true;
            if (_inUse == this)
                _inUse = null;
            _phase = Phase.Free;
            _getOffQueued = false;
            if (gotOff)
                PartnerOrNull?.PlayerGotOff();
        }

        void Switch(Phase phase)
        {
            _phase = phase;
            _timer = 0;
        }

        void UpdatePrompt()
        {
            if (Prompt == null)
                return;
            Prompt.Visible = InteractionFocus.IsFocused(this);
            if (!Prompt.Visible || Seat == null)
                return;
            var aboveSeat = Seat.GlobalPosition + Vector3.Up * PromptHeight;
            // seated: the E (get off) on the ground in front, the pump key above the seat
            Prompt.GlobalPosition = _phase == Phase.Seated && Enter != null
                ? Enter.GlobalPosition + Enter.GlobalBasis.Orthonormalized() * GetOffPromptOffset
                : aboveSeat;
            if (_pumpLabel != null)
            {
                _pumpLabel.Visible = _phase == Phase.Seated && !_jumpOffQueued && !_getOffQueued;
                if (_pumpLabel.Visible)
                    _pumpLabel.GlobalPosition = aboveSeat;
            }
        }

        static Vector3 Flat(Vector3 v) => new(v.X, 0, v.Z);
    }
}
