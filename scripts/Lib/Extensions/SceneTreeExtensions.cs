using Godot;
using System.Collections.Generic;

namespace TnT.Extensions
{
    public static class SceneTreeExtensions
    {
        // Group-based version
        public static List<T> FindObjectsByType<T>(this SceneTree tree, string groupName) where T : Node
        {
            var results = new List<T>();

            foreach (Node node in tree.GetNodesInGroup(groupName))
            {
                if (node is T tNode)
                    results.Add(tNode);
            }

            return results;
        }

        // Recursive version
        public static List<T> FindObjectsByType<T>(this SceneTree tree) where T : Node
        {
            var results = new List<T>();
            ScanTree(tree.Root, results);
            return results;

            void ScanTree(Node node, List<T> list)
            {
                if (node is T tNode)
                    list.Add(tNode);

                foreach (Node child in node.GetChildren())
                    ScanTree(child, list);
            }
        }
    }
}
