using System.Linq;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.Characters.States;
using TnT.EduGame.GameStates;
using TnT.EduGame.Interaction;
using TnT.EduGame.Inventory;
using TnT.EduGame.UI;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.Playground
{
    /// <summary>
    /// Something a <see cref="PlaythingSeat"/> needs before the player can get on, and that hears when they got off
    /// (the seesaw's other end, <see cref="AvocadoSpot"/>).
    /// </summary>
    public interface IPlaythingPartner
    {
        /// <summary>The player may get on now.</summary>
        bool IsReady { get; }
        /// <summary>It runs its own sequence with the player (placing or fetching Avocado): the seat waits.</summary>
        bool IsBusy { get; }
        /// <summary>The player pressed E at the seat while not <see cref="IsReady"/>: say why.</summary>
        void ExplainNotReady();
        /// <summary>The player got off the seat (stepped or jumped off; not when a quest took the player over).</summary>
        void PlayerGotOff();
    }

    /// <summary>
    /// The seesaw's other end, where the player puts Avocado. While the player carries Avocado (<see cref="AvocadoItem"/>
    /// in the inventory) and the end is empty, an E floats above it; E: the player walks to <see cref="Enter"/>, faces
    /// the seat and reaches (<see cref="PlaceState"/>), and an Avocado model (<see cref="AvocadoScene"/>) hops onto
    /// <see cref="Seat"/> (<c>avocado_seesaw_enter</c>). It then sits there, bouncing along with <see cref="Beam"/>
    /// (<c>avocado_seesaw_loop</c>, one cycle per bounce). After the player got off <see cref="PlayerSeat"/>, they
    /// walk over and take Avocado again (<see cref="PickupState"/>, <c>avocado_seesaw_exit</c>); E at a placed Avocado
    /// does the same. Avocado stays in the inventory all the time: the seesaw only shows a copy, so a save, a scene
    /// change or walking away can't lose it.
    /// </summary>
    [GlobalClass]
    public partial class AvocadoSpot : Area3D, IInteractable, IPlaythingPartner
    {
        const string DefaultPromptScene = "res://Scenes/interactables/quest_prompt.tscn";
        /// <summary>The AnimationPlayer with the seesaw clips in <see cref="AvocadoScene"/>.</summary>
        const string AvocadoPlayerName = "SeatedPlayer";
        const string EnterClip = "avocado_seesaw_enter", SitClip = "avocado_seesaw_loop", ExitClip = "avocado_seesaw_exit";

        [Export] public Node3D Enter { get; set; }
        /// <summary>Avocado's seat marker on the beam; the model is put under it.</summary>
        [Export] public Node3D Seat { get; set; }
        /// <summary>The player's end, which needs Avocado here before the player can sit down.</summary>
        [Export] public PlaythingSeat PlayerSeat { get; set; }
        [Export] public SeesawBeam Beam { get; set; }
        [Export] public ItemData AvocadoItem { get; set; }
        /// <summary>The light Avocado model (avocado.glb + an AnimationPlayer with the three seesaw clips).</summary>
        [Export] public PackedScene AvocadoScene { get; set; }
        /// <summary>Who says <see cref="NotPlacedText"/> / <see cref="NoAvocadoText"/> (Mabel).</summary>
        [Export] public CharacterData Speaker { get; set; }
        [Export(PropertyHint.MultilineText)] public string NotPlacedText { get; set; } = "Ik heb iemand nodig aan de andere kant. Avocado!";
        [Export(PropertyHint.MultilineText)] public string NoAvocadoText { get; set; } = "Wippen gaat niet alleen. Waar is Avocado?";
        [Export] public string PlaceState { get; set; } = "use";
        [Export] public string PickupState { get; set; } = "pickup";
        /// <summary>Seconds into the reach when Avocado appears (placing) or hops off (taking it back).</summary>
        [Export] public float HandOverSeconds { get; set; } = 0.45f;
        [Export] public float PromptHeight { get; set; } = 1.1f;
        [Export] public Node3D Prompt { get; set; }

        enum State { Empty, Placing, Placed, Fetching }
        enum Step { Approach, Reach }

        InputAction InteractAction => StateManagerGame.Instance?.PickupItemAction;
        bool _playerInRange;
        State _state = State.Empty;
        Step _step;
        double _timer;
        bool _handedOver;
        bool _holdDone = true;

        Node3D _avocado;
        AnimationPlayer _avocadoPlayer;
        bool _avocadoEntering;

        Character3D _body;
        CharacterController3D _cc;
        CharacterStateManager _states;
        CharacterAnimator _animator;
        PlayerApproach _approach;

        /// <summary>Avocado sits on the seesaw (placed, not being taken back).</summary>
        public bool IsPlaced => _state == State.Placed;

        public bool IsReady => _state == State.Placed;
        public bool IsBusy => _state is State.Placing or State.Fetching;

        bool Carrying => AvocadoItem != null && InventoryController.Instance?.model?.Items
            .Any(i => i != null && (i == AvocadoItem || (!string.IsNullOrEmpty(i.Id) && i.Id == AvocadoItem.Id))) == true;

        public override void _Ready()
        {
            if (Enter == null || Seat == null || AvocadoScene == null)
                GD.PrintErr($"AvocadoSpot '{Name}': Enter, Seat or AvocadoScene not set.");
            if (Prompt == null && GD.Load<PackedScene>(DefaultPromptScene) is PackedScene scene)
            {
                Prompt = scene.Instantiate<Node3D>();
                AddChild(Prompt);
            }
            if (Prompt != null)
            {
                Prompt.TopLevel = true;
                if (Prompt.FindAnyObjectByType<Label3D>(includeSelf: true) is Label3D label && InteractionFocus.KeyLabel(InteractAction) is string key)
                    label.Text = key;
            }
            BodyEntered += b => { if (Player.IsPlayer(b)) { _playerInRange = true; InteractionFocus.Add(this); } };
            BodyExited += b => { if (Player.IsPlayer(b)) { _playerInRange = false; InteractionFocus.Remove(this); } };
            UpdatePrompt();
        }

        public override void _ExitTree()
        {
            _playerInRange = false;
            InteractionFocus.Remove(this);
            if (_state is State.Placing or State.Fetching)
                Abort();
        }

        Vector3 IInteractable.InteractPosition => Enter?.GlobalPosition ?? GlobalPosition;

        bool IInteractable.CanInteract => _playerInRange && Enter != null && Seat != null && !PlayerBusy
            && ((_state == State.Empty && Carrying) || _state == State.Placed);

        bool PlayerBusy => PlayerSeat != null && PlayerSeat.IsBusy;

        public override void _Process(double delta)
        {
            UpdatePrompt();
            if (InteractionFocus.TryUse(this, InteractAction))
                Interact();
            UpdateAvocado();
        }

        /// <summary>Puts Avocado on (empty, carried) or takes it back (placed), as with the interact key.</summary>
        public void Interact()
        {
            if (PlayerBusy)
                return;
            if (_state == State.Empty && Carrying)
                Begin(State.Placing);
            else if (_state == State.Placed)
                Begin(State.Fetching);
        }

        public void ExplainNotReady()
        {
            if (Speaker == null || StateManagerGame.Instance == null)
                return;
            StateManagerGame.Instance.ShowMessage(Carrying ? NotPlacedText : NoAvocadoText, Speaker);
        }

        public void PlayerGotOff()
        {
            if (_state == State.Placed)
                Begin(State.Fetching);
        }

        void Begin(State state)
        {
            _body = Player.Instance?.FindAncestorOfType<Character3D>();
            _cc = _body?.FindAnyObjectByType<CharacterController3D>(includeSelf: true);
            _states = _body?.FindAnyObjectByType<CharacterStateManager>(includeSelf: true);
            _animator = _body?.FindAnyObjectByType<CharacterAnimator>(includeSelf: true);
            if (_cc == null || _states == null || _animator == null)
            {
                GD.PrintErr($"AvocadoSpot '{Name}': the player has no controller, state manager or animator.");
                return;
            }
            _state = state;
            _step = Step.Approach;
            _timer = 0;
            _handedOver = false;
            _holdDone = false;
            HoldPlayer();
            _approach = new PlayerApproach(_body, _cc, _body.FindAnyObjectByType<NavigationAgent3D>(includeSelf: true));
            _approach.Start(Enter.GlobalPosition, Seat.GlobalPosition - Enter.GlobalPosition);
        }

        async void HoldPlayer()
        {
            try
            {
                bool ended = await _states.Hold(() => _holdDone);
                // a quest took the player over meanwhile
                if (!ended && IsInstanceValid(this) && _state is State.Placing or State.Fetching)
                    Abort();
            }
            catch (System.Exception e)
            {
                GD.PushError($"AvocadoSpot '{Name}': {e}");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_state is not (State.Placing or State.Fetching))
                return;
            if (!IsInstanceValid(_body))
            {
                Abort();
                return;
            }
            _timer += delta;
            switch (_step)
            {
                case Step.Approach:
                    if (_approach.Step(delta))
                    {
                        _step = Step.Reach;
                        _timer = 0;
                        _animator.Hold(_state == State.Placing ? PlaceState : PickupState);
                    }
                    break;

                case Step.Reach:
                    if (!_handedOver && _timer >= HandOverSeconds)
                    {
                        _handedOver = true;
                        if (_state == State.Placing)
                            ShowAvocado();
                        else
                            PlayAvocado(ExitClip);
                    }
                    float reach = _animator.AnimationLength(_state == State.Placing ? PlaceState : PickupState);
                    float exit = _state == State.Fetching ? HandOverSeconds + AvocadoClipLength(ExitClip) : 0f;
                    if (_handedOver && _timer >= Mathf.Max(reach, exit))
                        Done();
                    break;
            }
        }

        void Done()
        {
            if (_state == State.Fetching)
                RemoveAvocado();
            _state = _state == State.Placing ? State.Placed : State.Empty;
            Release();
        }

        // interrupted: placing is undone, taking back finishes at once (Avocado is in the inventory either way)
        void Abort()
        {
            if (_state is State.Placing or State.Fetching)
            {
                RemoveAvocado();
                _state = State.Empty;
            }
            Release();
        }

        void Release()
        {
            if (IsInstanceValid(_animator))
                _animator.Release();
            _holdDone = true;
        }

        void ShowAvocado()
        {
            RemoveAvocado();
            if (AvocadoScene == null || Seat == null)
                return;
            _avocado = AvocadoScene.Instantiate<Node3D>();
            Seat.AddChild(_avocado);
            // by name: the avocado model has its own AnimationPlayer (idle, walk)
            _avocadoPlayer = _avocado.GetNodeOrNull<AnimationPlayer>(AvocadoPlayerName);
            PlayAvocado(EnterClip);
        }

        void PlayAvocado(string clip)
        {
            if (_avocadoPlayer == null || !_avocadoPlayer.HasAnimation(clip))
                return;
            _avocadoEntering = clip == EnterClip;
            _avocadoPlayer.Play(clip);
        }

        float AvocadoClipLength(string clip) =>
            _avocadoPlayer != null && _avocadoPlayer.HasAnimation(clip) ? (float)_avocadoPlayer.GetAnimation(clip).Length : 0f;

        void RemoveAvocado()
        {
            if (IsInstanceValid(_avocado))
                _avocado.QueueFree();
            _avocado = null;
            _avocadoPlayer = null;
            _avocadoEntering = false;
        }

        // sitting: the loop's pose at the beam's bounce (one loop cycle per bounce, its frame 0 = rest)
        void UpdateAvocado()
        {
            if (_avocadoPlayer == null || !IsInstanceValid(_avocadoPlayer) || _state == State.Fetching && _handedOver)
                return;
            if (_avocadoEntering)
            {
                if (_avocadoPlayer.IsPlaying() && _avocadoPlayer.CurrentAnimation == EnterClip)
                    return;
                _avocadoEntering = false;
            }
            if (!_avocadoPlayer.HasAnimation(SitClip))
                return;
            if (_avocadoPlayer.CurrentAnimation != SitClip)
                _avocadoPlayer.Play(SitClip);
            _avocadoPlayer.Pause();
            float progress = Beam != null ? Beam.Progress : 0f;
            _avocadoPlayer.Seek(progress * AvocadoClipLength(SitClip), true);
        }

        void UpdatePrompt()
        {
            if (Prompt == null)
                return;
            Prompt.Visible = InteractionFocus.IsFocused(this);
            if (Prompt.Visible && Seat != null)
                Prompt.GlobalPosition = Seat.GlobalPosition + Vector3.Up * PromptHeight;
        }
    }
}
