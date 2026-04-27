using Godot;
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
            foreach (var actionable in _actionables)
                actionable.Poll();
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
