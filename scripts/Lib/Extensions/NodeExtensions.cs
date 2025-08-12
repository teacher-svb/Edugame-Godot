using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{

    public static class NodeExtensions
    {
        /// <summary>
        /// Adds a new child node of type T to this node.
        /// The node is instantiated using the default constructor.
        /// </summary>
        public static T CreateChild<T>(this Node parent) where T : Node, new()
        {
            var child = new T();
            child.AddTo(parent);
            return child;
        }

        /// <summary>
        /// Creates a new child node of type T, adds it at the specified index in the parent's children,
        /// and returns the new node.
        /// </summary>
        public static T CreateChild<T>(this Node parent, int index) where T : Node, new()
        {
            var child = new T();
            child.AddTo(parent, index);
            return child;
        }

        /// <summary>
        /// Adds a new child node of type T to this node.
        /// The node is instantiated using the default constructor.
        /// </summary>
        public static T CreateChild<T>(this Node parent, string name) where T : Node, new()
        {
            var child = CreateChild<T>(parent);
            child.Name = name;
            return child;
        }

        /// <summary>
        /// Adds a new child node of type T to this node.
        /// The node is instantiated using the default constructor.
        /// </summary>
        public static T CreateChild<T>(this Node parent, string name, int index) where T : Node, new()
        {
            var child = CreateChild<T>(parent, index);
            child.Name = name;
            return child;
        }

        /// <summary>
        /// Adds this node as a child to the specified parent node.
        /// Returns the node itself to allow chaining.
        /// </summary>
        public static T AddTo<T>(this T child, Node parent) where T : Node
        {
            parent.AddChild(child);
            return child;
        }

        /// <summary>
        /// Adds this node as a child to the specified parent node at the given index.
        /// Returns the node itself for chaining.
        /// </summary>
        public static T AddTo<T>(this T child, Node parent, int index) where T : Node
        {
            parent.AddChild(child);
            parent.MoveChild(child, index);
            return child;
        }

        /// <summary>
        /// Removes all children of this node.
        /// </summary>
        public static void Clear(this Node parent)
        {
            var children = new List<Node>(parent.GetChildren());
            foreach (var child in children)
                child.QueueFree();
        }

        /// <summary>
        /// Removes all children of a specific type T.
        /// </summary>
        public static void Clear<T>(this Node parent) where T : Node
        {
            var children = parent.FindObjectsByType<T>();
            foreach (var child in children)
                child.QueueFree();
        }
    }
}