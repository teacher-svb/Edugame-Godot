using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace TnT.Input
{
    [Tool]
    public partial class InputManager : Node
    {
        public static InputManager Instance { get; private set; }

        private readonly List<InputActionReference> _actions = new();

        public override void _EnterTree()
        {
            Instance = this;

            // Watch for scene changes so we can auto-scan
            GetTree().NodeAdded += OnNodeAdded;
            GetTree().NodeRemoved += OnNodeRemoved;

            // Initial scan for already existing nodes
            ScanNodeAndChildren(GetTree().Root);
        }

        public override void _ExitTree()
        {
            GetTree().NodeAdded -= OnNodeAdded;
            GetTree().NodeRemoved -= OnNodeRemoved;

            Instance = null;
            _actions.Clear();
        }

        public override void _Process(double delta)
        {
            foreach (var action in _actions)
                action.Poll();
        }

        private void OnNodeAdded(Node node)
        {
            ScanNodeAndChildren(node);
        }

        private void OnNodeRemoved(Node node)
        {
            // Just in case: remove any references from removed nodes
            var toRemove = new List<InputActionReference>();
            foreach (var action in _actions)
            {
                if (action == null)
                    toRemove.Add(action);
            }
            foreach (var a in toRemove)
                _actions.Remove(a);
        }

        private void ScanNodeAndChildren(Node node)
        {
            ScanNode(node);

            foreach (Node child in node.GetChildren())
                ScanNodeAndChildren(child);
        }

        private void ScanNode(Node node)
        {
            // Look for any fields of type InputActionReference
            var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (typeof(InputActionReference).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(node) as InputActionReference;
                    if (value != null && !_actions.Contains(value))
                        _actions.Add(value);
                }
            }
        }
    }
}
