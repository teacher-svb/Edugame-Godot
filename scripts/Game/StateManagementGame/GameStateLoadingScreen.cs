using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.Extensions;

using TnT.EduGame.UI;
using TnT.EduGame.Characters;
namespace TnT.EduGame.GameStates
{
    [GlobalClass]
    public partial class GameStateLoadingScreen : BaseGameState, IStateObject<GameStateLoadingScreen.LoaderOptions>
    {
        string _currentScenePath;
        bool _loadingFinished = false;

        FadeController _fadeController;
        FadeController FadeController
        {
            get
            {
                if (_fadeController == null)
                    _fadeController = ManagerUI.Instance.FindAnyObjectByType<FadeController>();
                return _fadeController;
            }
        }

        public BaseState GetState(LoaderOptions options)
        {
            return GetState<LoaderOptions>(options);
        }

        public BaseState GetState<T>(T options) where T : ISceneLoaderOptions, new()
        {
            _loadingFinished = false;
            if (options is SceneLoaderOptions)
            {
                var sceneLoaderOptions = options as SceneLoaderOptions;
                if (_currentScenePath == null)
                    _currentScenePath = sceneLoaderOptions.scenePath;
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => _loadingFinished,
                    OnEnter = () => LoadScene(sceneLoaderOptions.scenePath, sceneLoaderOptions.targetLocation, sceneLoaderOptions.spawnPath, sceneLoaderOptions.onSceneReady),
                    OnExit = FadeOut
                });
            }
            else if (options is LocationLoaderOptions)
            {
                var sceneLoaderOptions = options as LocationLoaderOptions;
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => _loadingFinished,
                    OnEnter = () => LoadLocation(sceneLoaderOptions.targetLocation),
                    OnExit = FadeOut
                });
            }
            // else if (options is SceneLoaderOptions)
            //     return new BaseState(new() { ExitOnNextUpdate = () => true, OnEnter = FadeIn, OnExit = FadeOut });

            return BaseState.GetEmptyState();
        }

        async Task FadeIn()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await this.FadeController.ShowView();
        }

        async Task LoadLocation(Vector3 target)
        {
            await FadeIn();

            var tree = ManagerUI.Instance.GetTree();
            tree.FindAnyObjectByType<Player>().MoveTo(target);

            _loadingFinished = true;
        }

        async Task LoadScene(string scenePath, Vector3 target, NodePath spawnPath, Action<string> onSceneReady)
        {
            await FadeIn();

            var gameplayScene = GD.Load<PackedScene>(scenePath);
            ArgumentNullException.ThrowIfNull(gameplayScene, $"Scene not found at path: '{scenePath}'. Use full res:// path.");

            var tree = ManagerUI.Instance.GetTree();

            tree.Root.ChildEnteredTree += MovePlayer;
            tree.ChangeSceneToPacked(gameplayScene);

            async void MovePlayer(Node node)
            {
                tree.Root.ChildEnteredTree -= MovePlayer;

                await node.ToSignal(node, Node.SignalName.Ready);

                onSceneReady?.Invoke(scenePath);

                // after binding: a saved position (from the previous level) must not win over the spawn point
                if (spawnPath != null)
                {
                    var spawn = Levels.SpawnPoint.Find(tree, spawnPath, out bool missing);
                    if (missing)
                        GD.PushWarning($"{scenePath}: no spawn point at '{spawnPath}', using the default one");
                    if (spawn != null)
                        tree.FindAnyObjectByType<Player>().MoveTo(spawn.GlobalPosition);
                    else
                        GD.PushWarning($"{scenePath}: no default spawn point");
                }
                else if (target != Vector3.Zero)
                    tree.FindAnyObjectByType<Player>().MoveTo(target);
                _loadingFinished = true;
            }
        }

        async Task FadeOut()
        {
            await Task.Delay(1000);

            await this.FadeController.HideView();

            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = false;
        }

        public interface ISceneLoaderOptions { }

        public class LoaderOptions : ISceneLoaderOptions
        {
            // public bool forceLoad;
        }

        public class LocationLoaderOptions : LoaderOptions
        {
            // public Player player;
            public Vector3 targetLocation;
        }

        public class SceneLoaderOptions : LocationLoaderOptions
        {
            public string scenePath;
            /// <summary>
            /// The spawn point's node path relative to the scene root. null: no spawn point (a loaded save keeps its
            /// position); empty: the level's default one.
            /// </summary>
            public NodePath spawnPath;
            public Action<string> onSceneReady;
        }
    }
}