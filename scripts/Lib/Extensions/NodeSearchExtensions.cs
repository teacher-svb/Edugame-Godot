using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{
    /// <summary>
    /// Extension methods for searching the Godot scene tree by node type,
    /// inspired by Unity's <c>FindObjectsByType</c> / <c>FindAnyObjectByType</c> API.
    /// Searches first via Godot's group system (fast path) and fall back to a
    /// full recursive tree walk when no group match is found.
    /// </summary>
    public static class NodeSearchExtensions
    {
        // -----------------------
        // Public: Multiple results
        // -----------------------

        /// <summary>
        /// Returns all nodes of type <typeparamref name="T"/> in the scene tree.
        /// Uses a group-based fast path (group name == <c>typeof(T).Name</c>) when available,
        /// and falls back to a recursive walk from the root when not.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="tree">The scene tree to search.</param>
        /// <param name="recursive">
        /// When falling back to the recursive walk, whether to search all descendants
        /// (<c>true</c>) or only immediate children of the root (<c>false</c>).
        /// </param>
        /// <returns>A list of all matching nodes; empty if none are found.</returns>
        public static List<T> FindObjectsByType<T>(this SceneTree tree, bool recursive = true) where T : Node
        {
            // Try group fast path first
            var fastResults = FindObjectsByTypeFast<T>(tree);
            if (fastResults.Count > 0)
                return fastResults;

            // Fallback to recursive
            return tree.Root.FindObjectsByType<T>(recursive);
        }

        /// <summary>
        /// Returns all nodes of type <typeparamref name="T"/> that are descendants of <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="node">The root node to search from (its own children are searched, not the node itself).</param>
        /// <param name="recursive">
        /// <c>true</c> to search all descendants; <c>false</c> to check only immediate children.
        /// </param>
        /// <returns>A list of all matching nodes; empty if none are found.</returns>
        public static List<T> FindObjectsByType<T>(this Node node, bool recursive = true) where T : Node
        {
            var results = new List<T>();

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
        /// Returns the first node of type <typeparamref name="T"/> found in the scene tree.
        /// Uses a group-based fast path (group name == <c>typeof(T).Name</c>) when available,
        /// and falls back to a recursive walk from the root when not.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="tree">The scene tree to search.</param>
        /// <param name="recursive">
        /// When falling back to the recursive walk, whether to search all descendants
        /// (<c>true</c>) or only immediate children of the root (<c>false</c>).
        /// </param>
        /// <returns>The first matching node, or <c>null</c> if none is found.</returns>
        public static T FindAnyObjectByType<T>(this SceneTree tree, bool recursive = true) where T : Node
        {
            // Try group fast path first
            var fastResult = FindAnyObjectByTypeFast<T>(tree);
            if (fastResult != null)
                return fastResult;

            // Fallback to recursive
            return tree.Root.FindAnyObjectByType<T>(recursive);
        }

        /// <summary>
        /// Returns the first node of type <typeparamref name="T"/> found among the descendants
        /// of <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="T">The node type to search for.</typeparam>
        /// <param name="node">The root node to search from (its own children are searched, not the node itself).</param>
        /// <param name="recursive">
        /// <c>true</c> to search all descendants depth-first; <c>false</c> to check only immediate children.
        /// </param>
        /// <returns>The first matching node, or <c>null</c> if none is found.</returns>
        public static T FindAnyObjectByType<T>(this Node node, bool recursive = true) where T : Node
        {
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
        // Private: Fast group-based search
        // -----------------------
        private static List<T> FindObjectsByTypeFast<T>(SceneTree tree) where T : Node
        {
            string groupName = typeof(T).Name;
            var results = new List<T>();

            foreach (Node node in tree.GetNodesInGroup(groupName))
            {
                if (node is T tNode)
                    results.Add(tNode);
            }

            return results;
        }

        private static T FindAnyObjectByTypeFast<T>(SceneTree tree) where T : Node
        {
            string groupName = typeof(T).Name;

            foreach (Node node in tree.GetNodesInGroup(groupName))
            {
                if (node is T tNode)
                    return tNode;
            }

            return null;
        }

        // -----------------------
        // Private: Recursive helpers
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
