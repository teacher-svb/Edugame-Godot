using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{
    public static class SceneTreeExtensions
    {
        // ---------------------
        // Public "smart" methods
        // ---------------------

        public static List<T> FindObjectsByType<T>(this SceneTree tree) where T : Node
        {
            var results = tree.FindObjectsByTypeFast<T>();
            return results.Count > 0 ? results : tree.FindObjectsByTypeRecursive<T>();
        }

        public static T FindAnyObjectByType<T>(this SceneTree tree) where T : Node
        {
            var result = tree.FindAnyObjectByTypeFast<T>();
            return result != null ? result : tree.FindAnyObjectByTypeRecursive<T>();
        }

        // ---------------------
        // Private helper methods
        // ---------------------

        // Fast group-based
        private static List<T> FindObjectsByTypeFast<T>(this SceneTree tree) where T : Node
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

        // Slow recursive
        private static List<T> FindObjectsByTypeRecursive<T>(this SceneTree tree) where T : Node
        {
            var results = new List<T>();
            ScanTree(tree.Root, results);
            return results;

            static void ScanTree(Node node, List<T> list)
            {
                if (node is T tNode)
                    list.Add(tNode);

                foreach (Node child in node.GetChildren())
                    ScanTree(child, list);
            }
        }

        // Fast group-based single result
        private static T FindAnyObjectByTypeFast<T>(this SceneTree tree) where T : Node
        {
            string groupName = typeof(T).Name;

            foreach (Node node in tree.GetNodesInGroup(groupName))
            {
                if (node is T tNode)
                    return tNode;
            }

            return null;
        }

        // Slow recursive single result
        private static T FindAnyObjectByTypeRecursive<T>(this SceneTree tree) where T : Node
        {
            return ScanTreeForFirst<T>(tree.Root);

            static T ScanTreeForFirst<T>(Node node) where T : Node
            {
                if (node is T tNode)
                    return tNode;

                foreach (Node child in node.GetChildren())
                {
                    var result = ScanTreeForFirst<T>(child);
                    if (result != null)
                        return result;
                }

                return null;
            }
        }
    }
}
