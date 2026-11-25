using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{
    /// <summary>
    /// Extension methods for searching the Godot scene tree by node type,
    /// inspired by Unity's <c>FindObjectsByType</c> / <c>FindAnyObjectByType</c> API.
    /// Every search walks the tree depth-first, in child order. The node overloads take an explicit
    /// <c>includeSelf</c>: whether the start node itself is a candidate (tested first) or only its descendants.
    /// </summary>
    /// <remarks>
    /// There used to be a "fast path" through a group named like the type (<c>typeof(T).Name</c>). No group had such
    /// a name, and a group that happened to (e.g. a "Door" group) would have silently limited every search for that
    /// type to the group's members. Removed: every search walks the tree.
    /// </remarks>
    public static class NodeSearchExtensions
    {
        // -----------------------
        // Public: Multiple results
        // -----------------------

        /// <summary>
        /// Returns all nodes of type <typeparamref name="T"/> under the tree's root (the root window itself not
        /// included), in depth-first order.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="tree">The scene tree to search.</param>
        /// <param name="recursive">
        /// Whether to search all descendants (<c>true</c>) or only immediate children of the root (<c>false</c>).
        /// </param>
        /// <returns>A list of all matching nodes; empty if none are found.</returns>
        public static List<T> FindObjectsByType<T>(this SceneTree tree, bool recursive = true) where T : Node =>
            tree.Root.FindObjectsByType<T>(includeSelf: false, recursive);

        /// <summary>
        /// Returns all nodes of type <typeparamref name="T"/> in <paramref name="node"/>'s subtree, in depth-first order.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="node">The node to search from.</param>
        /// <param name="includeSelf">
        /// <c>true</c>: <paramref name="node"/> itself is a candidate (first in the list when it matches);
        /// <c>false</c>: only its descendants.
        /// </param>
        /// <param name="recursive">
        /// <c>true</c> to search all descendants; <c>false</c> to check only immediate children.
        /// </param>
        /// <returns>A list of all matching nodes; empty if none are found.</returns>
        public static List<T> FindObjectsByType<T>(this Node node, bool includeSelf, bool recursive = true) where T : Node
        {
            var results = new List<T>();

            if (includeSelf && node is T self)
                results.Add(self);

            if (recursive)
                ScanTree(node, results);
            else
            {
                foreach (Node child in node.GetChildren())
                {
                    if (child is T tNode)
                        results.Add(tNode);
                }
            }

            return results;
        }

        /// <summary>
        /// Walks up the scene tree and returns the first ancestor of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="node">The node whose ancestors are examined.</param>
        /// <param name="includeSelf">
        /// When <c>true</c>, <paramref name="node"/> itself is tested before walking up.
        /// Defaults to <c>false</c>.
        /// </param>
        /// <returns>The first matching ancestor, or <c>null</c> if none is found.</returns>
        public static T FindAncestorOfType<T>(this Node node, bool includeSelf = false) where T : Node
        {
            Node current = includeSelf ? node : node.GetParent();

            while (current != null)
            {
                if (current is T typed)
                    return typed;

                current = current.GetParent();
            }

            return null;
        }

        // -----------------------
        // Public: Single result
        // -----------------------

        /// <summary>
        /// Returns the first node of type <typeparamref name="T"/> under the tree's root (the root window itself not
        /// included), depth-first.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="tree">The scene tree to search.</param>
        /// <param name="recursive">
        /// Whether to search all descendants (<c>true</c>) or only immediate children of the root (<c>false</c>).
        /// </param>
        /// <returns>The first matching node, or <c>null</c> if none is found.</returns>
        public static T FindAnyObjectByType<T>(this SceneTree tree, bool recursive = true) where T : Node =>
            tree.Root.FindAnyObjectByType<T>(includeSelf: false, recursive);

        /// <summary>
        /// Returns the first node of type <typeparamref name="T"/> in <paramref name="node"/>'s subtree, depth-first.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="node">The node to search from.</param>
        /// <param name="includeSelf">
        /// <c>true</c>: <paramref name="node"/> itself is tested first (and returned when it matches);
        /// <c>false</c>: only its descendants.
        /// </param>
        /// <param name="recursive">
        /// <c>true</c> to search all descendants depth-first; <c>false</c> to check only immediate children.
        /// </param>
        /// <returns>The first matching node, or <c>null</c> if none is found.</returns>
        public static T FindAnyObjectByType<T>(this Node node, bool includeSelf, bool recursive = true) where T : Node
        {
            if (includeSelf && node is T self)
                return self;

            if (recursive)
                return ScanTreeForFirst<T>(node);

            foreach (Node child in node.GetChildren())
            {
                if (child is T tNode)
                    return tNode;
            }

            return null;
        }

        // -----------------------
        // Private: Recursive helpers (descendants only)
        // -----------------------
        private static void ScanTree<T>(Node current, List<T> list) where T : Node
        {
            foreach (Node child in current.GetChildren())
            {
                if (child is T tNode)
                    list.Add(tNode);

                ScanTree(child, list);
            }
        }

        private static T ScanTreeForFirst<T>(Node current) where T : Node
        {
            foreach (Node child in current.GetChildren())
            {
                if (child is T tNode)
                    return tNode;

                var result = ScanTreeForFirst<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
