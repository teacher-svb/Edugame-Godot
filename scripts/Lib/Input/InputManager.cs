using Godot;
using System;
using System.Collections.Generic;

namespace TnT.Input
{
    public partial class InputManager : Node
    {
        public static InputManager Instance { get; private set; }

        private readonly List<IInputActionable> _actionables = [];

        public override void _EnterTree()
        {
            Instance = this;

            GetTree().NodeAdded += OnNodeAdded;
            GetTree().NodeRemoved += OnNodeRemoved;
        }

        public override void _ExitTree()
        {
            GetTree().NodeAdded -= OnNodeAdded;
            GetTree().NodeRemoved -= OnNodeRemoved;

            Instance = null;
            _actionables.Clear();
        }

        public override void _Ready()
        {
            ScanNodeAndChildren(GetTree().Root);
        }

        
        /// <inheritdoc/>
        public override void _Process(double delta)
        {
            // a copy: a poll can free an actionable (NodeRemoved → Unregister) during the loop
            foreach (var actionable in _actionables.ToArray())
            {
                if (actionable is GodotObject obj && !GodotObject.IsInstanceValid(obj))
                    continue;
                // one failing actionable must not stop polling the rest for this frame
                try { actionable.Poll(); }
                catch (Exception e) { ReportOnce(actionable, e); }
            }
        }

        /// <summary>
        /// Polls <paramref name="action"/>; an exception (e.g. an <see cref="InputAction"/> naming an InputMap action
        /// that doesn't exist) is logged once per action instead of aborting the caller's loop. Null is skipped.
        /// </summary>
        public static void PollSafely(InputActionBase action)
        {
            if (action == null)
                return;
            try { action.Poll(); }
            catch (Exception e) { ReportOnce(action, e); }
        }

        // a broken action throws every frame: log it once, not 60 times a second
        static readonly HashSet<object> _reported = new(ReferenceEqualityComparer.Instance);

        static void ReportOnce(object source, Exception e)
        {
            if (!_reported.Add(source))
                return;
            string what = source switch
            {
                InputActionBase action => $"{action.GetType().Name} '{action.ActionName}'"
                    + (string.IsNullOrEmpty(action.ResourcePath) ? "" : $" ({action.ResourcePath})"),
                Node node when node.IsInsideTree() => node.GetPath().ToString(),
                _ => source.ToString(),
            };
            GD.PushError($"{nameof(InputManager)}: polling {what} failed (reported once): {e}");
        }

        /// <summary>Adds <paramref name="actionable"/> to the poll list if not already registered.</summary>
        public void Register(IInputActionable actionable)
        {
            if (!_actionables.Contains(actionable))
                _actionables.Add(actionable);
        }

        /// <summary>Removes <paramref name="actionable"/> from the poll list.</summary>
        public void Unregister(IInputActionable actionable)
        {
            _actionables.Remove(actionable);
        }

        private void OnNodeAdded(Node node)
        {
            if (node is IInputActionable actionable)
                actionable.Register();
        }

        private void OnNodeRemoved(Node node)
        {
            if (node is IInputActionable actionable)
                actionable.Unregister();
        }

        private void ScanNodeAndChildren(Node node)
        {
            if (node is IInputActionable actionable)
                actionable.Register();

            foreach (Node child in node.GetChildren())
                ScanNodeAndChildren(child);
        }
    }
}
