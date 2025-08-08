using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.Persistence
{
    [Tool]
    public abstract partial class SaveLoadSystem<[MustBeVariant] T> : Node where T : GameData, new()
    {
        protected IDataService<T> dataService;
        [Export] protected string _startSceneName;
        [Export] protected string _gameName;

        public T GameData;



        // [ExportToolButton("Save!")] // You can pass an icon as second argument if you want.
        // public Callable Save => Callable.From(SaveGame);

        public override void _Ready()
        {
            // base.Awake();
            dataService = new FileDataService<T>(new JsonSerializer());
        }

        void Start() => NewGame();

        // void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        // void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // protected abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);

        protected void Bind<U, TData>(ref TData data) where U : Node, IBind<TData> where TData : ISaveable, new()
        {
            var entity = GetTree().FindObjectsByType<U>().FirstOrDefault();
            if (entity == null)
                return;

            if (data == null)
                data = new TData { Id = entity.UniqueId.Id, IsNew = true };

            entity.Bind(data);
            data.IsNew = false;
            return;
        }

        protected void Bind<U, TData>(ref List<TData> datas) where U : Node, IBind<TData> where TData : ISaveable, new()
        {
            var entities = GetTree().FindObjectsByType<U>();

            foreach (var entity in entities)
            {
                var data = datas.FirstOrDefault(d => d.Id == entity.UniqueId.Id);
                if (data == null)
                {
                    data = new TData { Id = entity.UniqueId.Id, IsNew = true };
                    datas.Add(data);
                }
                entity.Bind(data);
                data.IsNew = false;
            }
        }

        public abstract void NewGame();

        public void SaveGame() => dataService.Save(GameData);

        public void ReloadGame() => LoadGame(GameData.Name);

        public abstract void LoadGame(string gameName);

        public void DeleteGame(string gameName) => dataService.Delete(gameName);
    }

    [Serializable]
    public abstract partial class GameData : GodotObject
    {
        public string Name;
    }

    public interface ISaveable
    {
        string Id { get; set; }
        bool IsNew { get; set; }
    }

    // [CustomPropertyDrawer(typeof(UniqueId))]
    // public class UniqueIdDrawer : PropertyDrawer
    // {
    //     public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //     {
    //         // Create property container element.
    //         var container = new VisualElement();

    //         var target = property.serializedObject.targetObject as IUnique;
    //         var copyFound = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IUnique>().Any(go => go != target && go.UniqueId.Id == target.UniqueId.Id);
    //         if (copyFound)
    //             target.UniqueId.Id = Guid.NewGuid().ToString();

    //         var idField = new PropertyField(property.FindPropertyRelative("Id"));
    //         container.Add(idField);

    //         return container;
    //     }
    // }

    [Serializable]
    public class UniqueId
    {
        [Export]
        public string Id = Guid.NewGuid().ToString();
    }

    public interface IUnique
    {
        UniqueId UniqueId { get; set; }
    }

    public interface IBind<TData> : IUnique where TData : ISaveable
    {
        void Bind(TData data);
    }

    public interface ISerializer
    {
        string Serialize(Variant obj);
        Variant Deserialize(string json);
    }

    public class JsonSerializer : ISerializer
    {
        public string Serialize(Variant obj)
        {
            return Json.Stringify(obj);
        }

        public Variant Deserialize(string json)
        {
            return Json.ParseString(json);
        }
    }

    public interface IDataService<T> where T : GameData, new()
    {
        void Save(T data, bool overwrite = true);
        T Load(string name);
        void Delete(string name);
        void DeleteAll();
        IEnumerable<string> ListSaves();
    }

    public class FileDataService<[MustBeVariant] T> : IDataService<T> where T : GameData, new()
    {
        ISerializer serializer;
        string dataPath;
        string fileExtension;

        public FileDataService(ISerializer serializer)
        {
            this.dataPath = ProjectSettings.GlobalizePath("user://"); //Application.persistentDataPath;
            this.fileExtension = "json";
            this.serializer = serializer;
        }

        string GetPathToFile(string fileName)
        {
            return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
        }

        public void Save(T data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.Name);

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"The file '{data.Name}.{fileExtension}' already exists and cannot be overwritten.");
            }

            GD.Print($"Saving GameData to '{fileLocation}'");
            Variant v = Variant.From(data);
            File.WriteAllText(fileLocation, serializer.Serialize(v));
        }

        public T Load(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (!File.Exists(fileLocation))
            {
                throw new ArgumentException($"No persisted GameData with name '{name}'");
            }

            GD.Print($"Loading GameData from '{fileLocation}'");
            return serializer.Deserialize(File.ReadAllText(fileLocation)).As<T>();
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (File.Exists(fileLocation))
            {
                GD.Print($"Deleting GameData from '{fileLocation}'");
                File.Delete(fileLocation);
            }
        }

        public void DeleteAll()
        {
            foreach (string filePath in Directory.GetFiles(dataPath))
            {
                File.Delete(filePath);
            }
        }

        public IEnumerable<string> ListSaves()
        {
            foreach (string path in Directory.EnumerateFiles(dataPath))
            {
                if (Path.GetExtension(path) == fileExtension)
                {
                    yield return Path.GetFileNameWithoutExtension(path);
                }
            }
        }
    }
}