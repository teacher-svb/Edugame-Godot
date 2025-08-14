// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Reflection;

// namespace TnT.Input
// {
//     public partial class InputManager : Node
//     {
//         public static InputManager Instance { get; private set; }

//         private readonly List<InputAction> _actions = new();

//         public override void _EnterTree()
//         {
//             Instance = this;

//             // Scan existing nodes
//             ScanNodeAndChildren(GetTree().Root);

//             // Watch for nodes added/removed at runtime
//             GetTree().NodeAdded += OnNodeAdded;
//             GetTree().NodeRemoved += OnNodeRemoved;
//         }

//         public override void _ExitTree()
//         {
//             GetTree().NodeAdded -= OnNodeAdded;
//             GetTree().NodeRemoved -= OnNodeRemoved;

//             Instance = null;
//             _actions.Clear();
//         }

//         public override void _Input(InputEvent ev)
//         {
//             foreach (var action in _actions)
//                 action.Poll();
//         }

//         private void OnNodeAdded(Node node)
//         {
//             ScanNodeAndChildren(node);
//         }

//         private void OnNodeRemoved(Node node)
//         {
//             // Remove null or destroyed actions from the list
//             _actions.RemoveAll(a => a == null);
//         }

//         private void ScanNodeAndChildren(Node node)
//         {
//             ScanNode(node);

//             foreach (Node child in node.GetChildren())
//                 ScanNodeAndChildren(child);
//         }

//         private void ScanNode(Node node)
//         {
//             var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//             foreach (var field in fields)
//             {
//                 if (typeof(InputAction).IsAssignableFrom(field.FieldType))
//                 {
//                     if (field.GetValue(node) is InputAction value && !_actions.Contains(value))
//                         _actions.Add(value);
//                 }
//             }
//         }
//     }
// }
