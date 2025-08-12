using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{
    public static class NodeSearchExtensions
    {
        // -----------------------
        // Public: Multiple results
        // -----------------------
        public static List<T> FindObjectsByType<T>(this SceneTree tree, bool recursive = true) where T : Node
        {
            // Try group fast path first
            var fastResults = FindObjectsByTypeFast<T>(tree);
            if (fastResults.Count > 0)
                return fastResults;

            // Fallback to recursive
            return tree.Root.FindObjectsByType<T>(recursive);
        }

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
        /// Finds the first ancestor (parent, grandparent, etc.) of type T.
        /// </summary>
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
        public static T FindAnyObjectByType<T>(this SceneTree tree, bool recursive = true) where T : Node
        {
            // Try group fast path first
            var fastResult = FindAnyObjectByTypeFast<T>(tree);
            if (fastResult != null)
                return fastResult;

            // Fallback to recursive
            return tree.Root.FindAnyObjectByType<T>(recursive);
        }

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
