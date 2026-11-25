using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Characters;
using TnT.Input;

namespace TnT.EduGame.Interaction
{
    /// <summary>
    /// Something in the world the interact key (<c>pickup</c>, E) works on while the player is in its range: an
    /// <c>Item</c>, a <c>Readable</c>. It adds itself to <see cref="InteractionFocus"/> when the player comes in range
    /// and removes itself when the player leaves (and when it leaves the tree).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Where it is (global), for the distance to the player.</summary>
        Vector3 InteractPosition { get; }

        /// <summary>Whether the key would do something now (enabled, available, not already open).</summary>
        bool CanInteract { get; }
    }

    /// <summary>
    /// Picks which interactable the interact key works on when the player is in range of several (an item next to a
    /// letter): the nearest one to the player that <see cref="IInteractable.CanInteract"/>, the first to come in range
    /// on a tie. Only that one shows its prompt and reacts, and a key press is used by one interactable only, also
    /// when the focus moves on in the same frame (an item picked up hands the focus to the letter next to it).
    /// </summary>
    public static class InteractionFocus
    {
        static readonly List<IInteractable> _inRange = new();
        static ulong _usedInFrame = ulong.MaxValue;

        /// <summary>The player came in range.</summary>
        public static void Add(IInteractable interactable)
        {
            if (interactable != null && !_inRange.Contains(interactable))
                _inRange.Add(interactable);
        }

        /// <summary>The player left its range, or it left the tree.</summary>
        public static void Remove(IInteractable interactable) => _inRange.Remove(interactable);

        /// <summary>The one the interact key works on now, or null.</summary>
        public static IInteractable Current
        {
            get
            {
                _inRange.RemoveAll(i => i is GodotObject o && !GodotObject.IsInstanceValid(o));
                var player = Player.Instance;
                Vector3? at = player != null && GodotObject.IsInstanceValid(player) && player.GetParentOrNull<Node3D>() is Node3D body
                    ? body.GlobalPosition
                    : null;
                IInteractable best = null;
                float bestDistance = float.MaxValue;
                foreach (var candidate in _inRange.Where(i => i.CanInteract))
                {
                    float distance = at is Vector3 p ? p.DistanceSquaredTo(candidate.InteractPosition) : 0f;
                    if (best == null || distance < bestDistance)
                    {
                        best = candidate;
                        bestDistance = distance;
                    }
                }
                return best;
            }
        }

        public static bool IsFocused(IInteractable interactable) => interactable != null && Current == interactable;

        /// <summary>
        /// True when <paramref name="action"/> triggered this frame, <paramref name="interactable"/> has the focus and no
        /// other interactable used this press yet; it then counts as used.
        /// </summary>
        public static bool TryUse(IInteractable interactable, InputAction action)
        {
            if (action?.Triggered != true || !IsFocused(interactable))
                return false;
            ulong frame = Engine.GetProcessFrames();
            if (_usedInFrame == frame)
                return false;
            _usedInFrame = frame;
            return true;
        }

        /// <summary>
        /// The key of <paramref name="action"/>'s first keyboard binding as shown on this keyboard layout (e.g. "E"),
        /// for a prompt label; null without one. The headless display server (tests, CI) has no layout: the physical
        /// key's name then.
        /// </summary>
        public static string KeyLabel(InputActionBase action)
        {
            if (action == null || string.IsNullOrEmpty(action.ActionName) || !InputMap.HasAction(action.ActionName)
                || InputMap.ActionGetEvents(action.ActionName).FirstOrDefault(e => e is InputEventKey) is not InputEventKey key)
                return null;
            var keycode = DisplayServer.GetName() == "headless"
                ? key.PhysicalKeycode
                : DisplayServer.KeyboardGetKeycodeFromPhysical(key.PhysicalKeycode);
            return OS.GetKeycodeString(keycode);
        }
    }
}
