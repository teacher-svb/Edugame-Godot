using System.Linq;
using Godot;
using TnT.EduGame.GameStates;
using TnT.EduGame.QuestSystem;
using TnT.Extensions;
using TnT.Input;
using TnT.EduGame.Characters;
using TnT.EduGame.Interaction;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// An item lying in a level. While the player is inside <see cref="_area"/>, the interact key (<c>pickup</c>, E,
    /// shared with <c>Readable</c>) sends <see cref="Data"/> over <see cref="_pickupChannel"/> (the inventory and,
    /// e.g., a quest objective listen there) and removes the item's root (its parent node) from the world. The pickup
    /// is refused, and the item stays, when a listener can't take it (a full inventory). A picked-up item is
    /// remembered in the save data (scene-scoped id), so it stays gone after a load and when the level is entered again.
    /// In range of several interactables (an item next to a letter), only the nearest (<see cref="InteractionFocus"/>)
    /// reacts to the key; this item's prompt then hides while another one has the focus.
    /// </summary>
    [GlobalClass]
    public partial class Item : Node3D, IBind<WorldItemSaveData>, IInteractable
    {
        [Export] ItemData _staticData;
        [Export] ItemEventChannel _pickupChannel;
        /// <summary>The pickup range; empty: the first <see cref="Area3D"/> under this node.</summary>
        [Export] Area3D _area;
        /// <summary>
        /// Optional: the item can't be picked up (and shows no prompt) while this objective of <see cref="_quest"/>
        /// hasn't started, so a pickup that completes that objective can't happen too early and be lost.
        /// </summary>
        [Export] Quest _quest;
        [Export] string _availableFromObjectiveId;

        InputAction InteractAction => StateManagerGame.Instance?.PickupItemAction;

        bool _playerInRange;
        bool _pickedUp;
        WorldItemSaveData _saveData;

        public ItemData Data => _staticData;

        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time

        public override void _Ready()
        {
            _area ??= this.FindAnyObjectByType<Area3D>(includeSelf: true);
            if (_area != null)
            {
                _area.BodyEntered += OnBodyEntered;
                _area.BodyExited += OnBodyExited;
            }
            else
                GD.PushWarning($"Item '{GetPath()}': no Area3D for the pickup range, it can't be picked up");

            if (this.FindAnyObjectByType<Label3D>(includeSelf: true) is Label3D label && InteractionFocus.KeyLabel(InteractAction) is string key)
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
            UpdatePrompt();
            if (InteractionFocus.TryUse(this, InteractAction))
                Pickup();
        }

        Vector3 IInteractable.InteractPosition => _area?.GlobalPosition ?? GlobalPosition;
        bool IInteractable.CanInteract => _playerInRange && !_pickedUp && IsAvailable;

        /// <summary>False before <see cref="_availableFromObjectiveId"/> started (no quest set: always true).</summary>
        public bool IsAvailable
        {
            get
            {
                if (_quest == null || string.IsNullOrEmpty(_availableFromObjectiveId))
                    return true;
                if (_availableFrom == null)
                {
                    _availableFrom = _quest.Objectives.FirstOrDefault(o => o.ObjectiveId == _availableFromObjectiveId);
                    if (_availableFrom == null)
                    {
                        GD.PushWarning($"Item '{Name}': quest '{_quest.Name}' has no objective '{_availableFromObjectiveId}', the item is always available");
                        _quest = null;
                        return true;
                    }
                }
                return _availableFrom.State != QuestState.NOTSTARTED;
            }
        }
        QuestObjective _availableFrom;

        // the prompt (this node's children, e.g. "druk op E") only while it can be picked up, and not while the player
        // is in range of a nearer interactable that the key would work on instead
        void UpdatePrompt()
        {
            bool show = IsAvailable && !_pickedUp && (!_playerInRange || InteractionFocus.IsFocused(this));
            if (Visible != show)
                Visible = show;
        }

        void OnBodyEntered(Node body)
        {
            if (Player.IsPlayer(body))
            {
                _playerInRange = true;
                InteractionFocus.Add(this);
            }
        }

        void OnBodyExited(Node body)
        {
            if (Player.IsPlayer(body))
            {
                _playerInRange = false;
                InteractionFocus.Remove(this);
            }
        }

        /// <summary>
        /// Picks the item up: false (the item stays) when a listener refuses it, e.g. the inventory is full.
        /// </summary>
        public bool Pickup()
        {
            if (_pickedUp || _staticData == null || _pickupChannel == null)
                return false;
            if (!_pickupChannel.CanDeliver(_staticData))
                return false;   // e.g. the inventory is full: the item stays where it is
            _pickedUp = true;
            if (_saveData != null)
                _saveData.IsPickedUp = true;
            _pickupChannel.Invoke(_staticData);
            RemoveFromWorld();
            return true;
        }

        public void Bind(WorldItemSaveData data)
        {
            _saveData = data;
            if (data.IsPickedUp)
            {
                _pickedUp = true;
                RemoveFromWorld();
            }
        }

        // the item's root is its parent (the avocado character); an item placed directly in the level frees only itself
        void RemoveFromWorld()
        {
            var parent = GetParent();
            var root = parent == null || parent == GetTree().CurrentScene ? (Node)this : parent;
            root.QueueFree();
        }
    }
}
