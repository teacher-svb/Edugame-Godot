using System.Linq;
using Godot;
using TnT.EduGame.GameState;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.Reading
{
    /// <summary>
    /// Something in the world the player can read (a letter on the table, a notice on a board): while the player is
    /// inside this area, <see cref="Prompt"/> shows the interact key, and pressing it opens <see cref="Text"/> in the
    /// reading panel. <see cref="Read"/> fires each time it is closed after reaching the last page: connect it to
    /// <c>QuestNode.CompleteObjective</c> with the objective id, or wait for it with a <c>ReadStep</c>.
    /// </summary>
    [GlobalClass]
    public partial class Readable : Area3D
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
            BodyEntered += b => { if (IsPlayer(b)) { _playerInRange = true; UpdatePrompt(); } };
            BodyExited += b => { if (IsPlayer(b)) { _playerInRange = false; UpdatePrompt(); } };
            if (Prompt?.FindAnyObjectByType<Label3D>() is Label3D label && InteractAction != null
                && InputMap.ActionGetEvents(InteractAction.ActionName).FirstOrDefault(e => e is InputEventKey) is InputEventKey key)
                label.Text = OS.GetKeycodeString(DisplayServer.KeyboardGetKeycodeFromPhysical(key.PhysicalKeycode));
            UpdatePrompt();
        }

        public override void _Process(double delta)
        {
            if (_playerInRange && Enabled && !_reading && InteractAction?.Triggered == true)
                Interact();
        }

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
                Prompt.Visible = Enabled && _playerInRange && !_reading;
        }

        static bool IsPlayer(Node body) => body.FindAnyObjectByType<Player>() != null;
    }
}
