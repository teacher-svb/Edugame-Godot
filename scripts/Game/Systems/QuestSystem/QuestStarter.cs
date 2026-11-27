using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.GameStates;
using TnT.EduGame.Interaction;
using TnT.Extensions;
using TnT.Input;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// The start area of a quest an NPC gives: while the player is inside it, a key label ("E") floats above
    /// <see cref="Giver"/>, and pressing the interact key starts <see cref="ObjectiveId"/> on <see cref="QuestNode"/>.
    /// Replaces a plain start <c>Area3D</c> wired with <c>body_entered</c> → <c>_OnPlayerStartObjective</c>; arm it the
    /// same way (<c>set_Monitoring</c> from a listener). The key is shared with items and readables: in range of several,
    /// only the nearest (<see cref="InteractionFocus"/>, measured to the giver) shows its prompt and reacts.
    /// </summary>
    [GlobalClass]
    public partial class QuestStarter : Area3D, IInteractable
    {
        const string DefaultPromptScene = "res://Scenes/interactables/quest_prompt.tscn";

        [Export] public QuestNode QuestNode { get; set; }
        [Export] public string ObjectiveId { get; set; } = "";
        /// <summary>The NPC who gives the quest; the prompt floats above them and follows them when they walk.
        /// Empty: the prompt stays above this area's origin.</summary>
        [Export] public Node3D Giver { get; set; }
        /// <summary>Height of the prompt above the giver's origin (their feet).</summary>
        [Export] public float PromptHeight { get; set; } = 2.3f;
        /// <summary>Shown while the player can start the quest; empty: an instance of <c>quest_prompt.tscn</c>.</summary>
        [Export] public Node3D Prompt { get; set; }

        InputAction InteractAction => StateManagerGame.Instance?.PickupItemAction;
        bool _playerInRange;

        public override void _Ready()
        {
            if (QuestNode == null || string.IsNullOrEmpty(ObjectiveId))
                GD.PrintErr($"QuestStarter '{Name}': QuestNode or ObjectiveId not set.");

            if (Prompt == null && GD.Load<PackedScene>(DefaultPromptScene) is PackedScene scene)
            {
                Prompt = scene.Instantiate<Node3D>();
                AddChild(Prompt);
            }
            if (Prompt != null)
            {
                Prompt.TopLevel = true;   // placed in world space above the giver, not moved with this area
                // headless-safe, like Item and Readable: the headless display server has no keyboard layout
                if (Prompt.FindAnyObjectByType<Label3D>(includeSelf: true) is Label3D label && InteractionFocus.KeyLabel(InteractAction) is string key)
                    label.Text = key;
            }

            BodyEntered += b => { if (Player.IsPlayer(b)) { _playerInRange = true; InteractionFocus.Add(this); UpdatePrompt(); } };
            BodyExited += b => { if (Player.IsPlayer(b)) { _playerInRange = false; InteractionFocus.Remove(this); UpdatePrompt(); } };
            UpdatePrompt();
        }

        public override void _ExitTree()
        {
            _playerInRange = false;
            InteractionFocus.Remove(this);
        }

        public override void _Process(double delta)
        {
            UpdatePrompt();   // the focus moves as the player walks, the giver may walk, the objective may start elsewhere
            if (InteractionFocus.TryUse(this, InteractAction))
                Interact();
        }

        Vector3 IInteractable.InteractPosition => IsInstanceValid(Giver) ? Giver.GlobalPosition : GlobalPosition;
        bool IInteractable.CanInteract => _playerInRange && Monitoring && CanStart;

        /// <summary>The objective hasn't started yet and no transition of it waits for its message.</summary>
        bool CanStart => QuestNode?.FindObjective(ObjectiveId) is QuestObjective o
                         && o.State == QuestState.NOTSTARTED && !o.HasPendingTransition;

        /// <summary>Starts the objective, as if the player pressed the interact key.</summary>
        public void Interact()
        {
            if (!CanStart)
                return;
            QuestNode.StartObjective(ObjectiveId);
            UpdatePrompt();
        }

        void UpdatePrompt()
        {
            if (Prompt == null)
                return;
            Prompt.Visible = InteractionFocus.IsFocused(this);   // in range, armed, not started, and the nearest
            if (Prompt.Visible)
                Prompt.GlobalPosition = (IsInstanceValid(Giver) ? Giver.GlobalPosition : GlobalPosition) + Vector3.Up * PromptHeight;
        }
    }
}
