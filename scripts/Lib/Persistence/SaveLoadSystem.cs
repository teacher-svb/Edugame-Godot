using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using TnT.Extensions;
using Newtonsoft.Json;

namespace TnT.Systems.Persistence
{
    public abstract partial class SaveLoadSystem<T> : Node where T : GameData, new()
    {
        protected IDataService<T> dataService;
        [Export(PropertyHint.File,"*.tscn,*.tsc")] protected string _startScene;
        [Export] protected string _gameName;

        public abstract T GameData { get; set; }

        public override void _Ready()
        {
            dataService = new FileDataService<T>();
        }

        protected void Bind<U, TData>(ref TData data) where U : Node, IBind<TData> where TData : ISaveable, new()
        {
            var entity = GetTree().FindObjectsByType<U>().FirstOrDefault();
            if (entity == null)
                return;

            if (data == null)
                data = new TData { Id = entity.PersistentId, IsNew = true };

            entity.Bind(data);
            data.IsNew = false;
            return;
        }

        protected void Bind<U, TData>(ref List<TData> datas) where U : Node, IBind<TData> where TData : ISaveable, new() =>
            Bind<U, TData>(datas, null);

        // Binds the entities in `root`'s subtree, root included (the whole tree when null). An entity
        // already in `datas` (same PersistentId) gets that entry again, so binding twice adds nothing.
        protected void Bind<U, TData>(List<TData> datas, Node root) where U : Node, IBind<TData> where TData : ISaveable, new()
        {
            var entities = root == null ? GetTree().FindObjectsByType<U>() : root.FindObjectsByType<U>(includeSelf: true);

            foreach (var entity in entities)
            {
                var data = datas.FirstOrDefault(d => d.Id == entity.PersistentId);
                if (data == null)
                {
                    data = new TData { Id = entity.PersistentId, IsNew = true };
                    datas.Add(data);
                }
                entity.Bind(data);
                data.IsNew = false;
            }
        }

        public abstract void NewGame();

        /// <summary>
        /// Writes <see cref="GameData"/> to its save file. Override to copy state into it first. False (logged) when
        /// the file can't be written; the previous save stays.
        /// </summary>
        public virtual bool SaveGame()
        {
            try
            {
                dataService.Save(GameData);
                return true;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                GD.PushError($"Saving '{GameData?.Name}' failed: {e.Message}");
                return false;
            }
        }

        /// <summary>The game's own save name: what a new game saves as. The running game may use another (<see cref="GameData"/>'s <c>Name</c>).</summary>
        public string MainGameName => _gameName;

        /// <summary>True when a save file (or its backup) exists for the running game's name.</summary>
        public bool HasSave() => HasSave(GameData?.Name ?? _gameName);

        /// <summary>True when a save file (or its backup) named <paramref name="gameName"/> exists.</summary>
        public bool HasSave(string gameName) => dataService.Exists(gameName);

        public bool ReloadGame() => LoadGame(GameData?.Name ?? _gameName);

        /// <summary>
        /// Loads the save <paramref name="gameName"/> and starts it. False (logged, nothing changed) when there is
        /// no save or it can't be read.
        /// </summary>
        public abstract bool LoadGame(string gameName);

        public void DeleteGame(string gameName) => dataService.Delete(gameName);
    }

    public interface ISaveable
    {
        string Id { get; set; }
        bool IsNew { get; set; }
    }

    public interface IUnique
    {
        string PersistentId { get; }
    }

    public interface IBind<TData> : IUnique where TData : ISaveable
    {
        void Bind(TData data);
    }

    /// <summary>Save keys for nodes that belong to a level.</summary>
    public static class PersistentIds
    {
        /// <summary>
        /// <c>"&lt;current scene file&gt;#&lt;localId&gt;"</c>, the node's path relative to the current scene when
        /// <paramref name="localId"/> is null. Unique across levels, also when two levels share a root node name.
        /// Read it while the node is in the tree (at bind time), not in _EnterTree: the current scene may not be
        /// set yet then.
        /// </summary>
        public static string SceneScoped(Node node, string localId = null)
        {
            if (!node.IsInsideTree())
                return $"#{localId ?? node.Name.ToString()}";
            var scene = node.GetTree().CurrentScene;
            if (localId == null)
            {
                bool inScene = scene != null && (scene == node || scene.IsAncestorOf(node));
                localId = inScene ? scene.GetPathTo(node).ToString() : node.GetPath().ToString();
            }
            return $"{scene?.SceneFilePath}#{localId}";
        }
    }

    public interface IDataService<T> where T : GameData, new()
    {
        void Save(T data, bool overwrite = true);
        /// <summary>False (logged) when neither the save nor its backup exists or can be read.</summary>
        bool TryLoad(string name, out T data);
        bool Exists(string name);
        void Delete(string name);
        void DeleteAll();
        IEnumerable<string> ListSaves();
    }

    /// <summary>
    /// One JSON file per save in <c>user://</c> (<c>&lt;name&gt;.json</c>). A save is written to
    /// <c>&lt;name&gt;.json.tmp</c> first and then moved over the save, the previous save kept as
    /// <c>&lt;name&gt;.json.bak</c>; a crash mid-save leaves the old save intact. Loading falls back to the
    /// backup when the save is missing or can't be parsed.
    /// </summary>
    public class FileDataService<T> : IDataService<T> where T : GameData, new()
    {
        const string TempSuffix = ".tmp";
        const string BackupSuffix = ".bak";

        readonly string dataPath;
        readonly string fileExtension;

        public FileDataService(string dataPath = null, string fileExtension = "json")
        {
            this.dataPath = dataPath ?? ProjectSettings.GlobalizePath("user://");
            this.fileExtension = fileExtension;
        }

        string GetPathToFile(string fileName)
        {
            return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
        }

        public void Save(T data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.Name);
            string tempLocation = fileLocation + TempSuffix;
            string backupLocation = fileLocation + BackupSuffix;

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"The file '{data.Name}.{fileExtension}' already exists and cannot be overwritten.");
            }

            GD.Print($"Saving GameData to '{fileLocation}'");
            var json = JsonConvert.SerializeObject(data);
            using (var stream = new FileStream(tempLocation, FileMode.Create, System.IO.FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(true);   // on disk before the move, so the move can't publish an empty file
            }

            // the previous save becomes the backup, unless it is unreadable: then the backup (the last good save) stays
            if (File.Exists(fileLocation) && TryRead(fileLocation, out _, out _))
                File.Replace(tempLocation, fileLocation, backupLocation);
            else
                File.Move(tempLocation, fileLocation, true);
        }

        public bool TryLoad(string name, out T data)
        {
            string fileLocation = GetPathToFile(name);
            string backupLocation = fileLocation + BackupSuffix;

            if (TryRead(fileLocation, out data, out var error))
            {
                GD.Print($"Loaded GameData from '{fileLocation}'");
                return true;
            }
            if (TryRead(backupLocation, out data, out var backupError))
            {
                GD.PushWarning($"Save '{fileLocation}' unreadable ({error}); loaded the backup '{backupLocation}'");
                return true;
            }
            GD.PushError($"No loadable GameData with name '{name}': {error}; backup: {backupError}");
            return false;
        }

        // the file parsed into a non-null T; any IO or JSON problem is reported in `error`
        static bool TryRead(string fileLocation, out T data, out string error)
        {
            data = null;
            if (!File.Exists(fileLocation))
            {
                error = "file not found";
                return false;
            }
            try
            {
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileLocation));
                error = data == null ? "empty file" : null;
                return data != null;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException
                                          or ArgumentException or InvalidCastException or FormatException)
            {
                error = $"{e.GetType().Name}: {e.Message}";
                data = null;
                return false;
            }
        }

        public bool Exists(string name)
        {
            string fileLocation = GetPathToFile(name);
            return File.Exists(fileLocation) || File.Exists(fileLocation + BackupSuffix);
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);

            foreach (var path in new[] { fileLocation, fileLocation + TempSuffix, fileLocation + BackupSuffix })
            {
                if (File.Exists(path))
                {
                    GD.Print($"Deleting '{path}'");
                    File.Delete(path);
                }
            }
        }

        /// <summary>Deletes every save (with its backup and temp file); other files in the folder stay.</summary>
        public void DeleteAll()
        {
            string save = "." + fileExtension;
            foreach (string path in Directory.GetFiles(dataPath))
            {
                if (path.EndsWith(save) || path.EndsWith(save + TempSuffix) || path.EndsWith(save + BackupSuffix))
                    File.Delete(path);
            }
        }

        public IEnumerable<string> ListSaves()
        {
            foreach (string path in Directory.EnumerateFiles(dataPath))
            {
                // Path.GetExtension includes the dot; "x.json.bak" and "x.json.tmp" don't count
                if (Path.GetExtension(path) == "." + fileExtension)
                {
                    yield return Path.GetFileNameWithoutExtension(path);
                }
            }
        }
    }
}
