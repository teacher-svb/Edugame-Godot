using Godot;
using TnT.EduGame.GameStates;
using TnT.Extensions;
using TnT.Input;
using TnT.EduGame.Characters;
using TnT.EduGame.Interaction;

namespace TnT.EduGame.Reading
{
    /// <summary>
    /// Something in the world the player can read (a letter on the table, a notice on a board): while the player is
    /// inside this area, <see cref="Prompt"/> shows the interact key, and pressing it opens <see cref="Text"/> in the
    /// reading panel. <see cref="Read"/> fires each time it is closed after reaching the last page: connect it to
    /// <c>QuestNode.CompleteObjective</c> with the objective id, or wait for it with a <c>ReadStep</c>. The interact key is
    /// shared with items: in range of several, only the nearest (<see cref="InteractionFocus"/>) shows its prompt and reacts.
    /// </summary>
    [GlobalClass]
    public partial class Readable : Area3D, IInteractable
    {
        [Export] public ReadableText Text { get; set; }
        /// <summary>Shown while the player can read this (e.g. a Label3D with the key); optional.</summary>
        [Export] public Node3D Prompt { get; set; }
        /// <summary>A closed readable shows no prompt and can't be opened (a quest opens it with <see cref="Open"/>).</summary>
        [Export] public bool Enabled { get; set; } = true;

        [Signal] public delegate void OpenedEventHandler();
        [Signal] public delegate void ClosedEventHandler(bool readToEnd);
        [Signal] public delegate void ReadEventHandler();

        public bool HasBeenRead { get; private set; }

        InputAction InteractAction => StateManagerGame.Instance?.PickupItemAction;
        bool _playerInRange;
        bool _reading;

        public override void _Ready()
        {
            BodyEntered += b => { if (Player.IsPlayer(b)) { _playerInRange = true; InteractionFocus.Add(this); UpdatePrompt(); } };
            BodyExited += b => { if (Player.IsPlayer(b)) { _playerInRange = false; InteractionFocus.Remove(this); UpdatePrompt(); } };
            // headless-safe, like Item: the headless display server has no keyboard layout
            if (Prompt?.FindAnyObjectByType<Label3D>(includeSelf: true) is Label3D label && InteractionFocus.KeyLabel(InteractAction) is string key)
                label.Text = key;
            UpdatePrompt();
        }

        public override void _ExitTree()
        {
            _playerInRange = false;
            InteractionFocus.Remove(this);
        }

        public override void _Process(double delta)
        {
            UpdatePrompt();   // the focus moves as the player walks between interactables
            if (InteractionFocus.TryUse(this, InteractAction))
                Interact();
        }

        Vector3 IInteractable.InteractPosition => GlobalPosition;
        bool IInteractable.CanInteract => _playerInRange && Enabled && !_reading;

        /// <summary>Opens the text in the reading panel, as if the player pressed the interact key.</summary>
        public void Interact()
        {
            if (Text == null || _reading)
                return;
            _reading = true;
            UpdatePrompt();
            EmitSignal(SignalName.Opened);
            StateManagerGame.Instance.ShowReading(Text, readToEnd =>
            {
                _reading = false;
                UpdatePrompt();
                EmitSignal(SignalName.Closed, readToEnd);
                if (readToEnd)
                {
                    HasBeenRead = true;
                    EmitSignal(SignalName.Read);
                }
            });
        }

        public void Open() { Enabled = true; UpdatePrompt(); }
        public void Close() { Enabled = false; UpdatePrompt(); }

        void UpdatePrompt()
        {
            if (Prompt != null)
                Prompt.Visible = InteractionFocus.IsFocused(this);   // in range, enabled, not open, and the nearest
        }

    }
}
