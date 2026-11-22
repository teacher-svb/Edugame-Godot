using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// Where the player arrives in a level through a <see cref="LevelExit"/>: the exit stores the spawn point's node
    /// path in its target scene (picked from a dropdown). An exit without a spawn path uses the level's
    /// <see cref="Default"/> one. Keep spawn points out of exit areas, or the player walks straight back out (exits
    /// also ignore a player that is already inside them when the level starts).
    /// </summary>
    [Tool, GlobalClass]
    public partial class SpawnPoint : Marker3D
    {
        /// <summary>Used by exits that name no spawn point (one per level).</summary>
        [Export] public bool Default { get; set; }

        /// <summary>
        /// The spawn point at <paramref name="path"/> (relative to the current scene's root), else the default one,
        /// else null. <paramref name="missing"/> is true when a path was given but no spawn point is there.
        /// </summary>
        public static SpawnPoint Find(SceneTree tree, NodePath path, out bool missing)
        {
            missing = false;
            if (path != null && !path.IsEmpty)
            {
                if (tree.CurrentScene?.GetNodeOrNull(path) is SpawnPoint named)
                    return named;
                missing = true;
            }
            return tree.FindObjectsByType<SpawnPoint>().FirstOrDefault(p => p.Default);
        }

        /// <summary>
        /// The spawn points saved in a scene file (node path relative to its root, and whether it is the default),
        /// read from its <see cref="SceneState"/> without instancing it. Spawn points inside instanced sub-scenes are
        /// not listed.
        /// </summary>
        public static List<(string Path, bool IsDefault)> ListIn(string scenePath)
        {
            var spawns = new List<(string, bool)>();
            if (string.IsNullOrEmpty(scenePath) || !ResourceLoader.Exists(scenePath))
                return spawns;
            var state = ResourceLoader.Load<PackedScene>(scenePath)?.GetState();
            if (state == null)
                return spawns;

            for (int i = 0; i < state.GetNodeCount(); i++)
            {
                bool isSpawn = false, isDefault = false;
                for (int p = 0; p < state.GetNodePropertyCount(i); p++)
                {
                    var name = state.GetNodePropertyName(i, p);
                    var value = state.GetNodePropertyValue(i, p);
                    if (name == "script" && value.As<Script>() is { } script && script.GetGlobalName() == nameof(SpawnPoint))
                        isSpawn = true;
                    else if (name == PropertyName.Default)
                        isDefault = value.AsBool();
                }
                if (isSpawn)
                {
                    var path = state.GetNodePath(i).ToString();
                    spawns.Add((path.StartsWith("./") ? path[2..] : path, isDefault));
                }
            }
            return spawns;
        }
    }
}
