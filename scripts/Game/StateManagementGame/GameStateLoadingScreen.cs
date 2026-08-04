using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.Systems.UI;
using TnT.Extensions;

namespace TnT.EduGame.GameState
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
                    OnEnter = () => LoadScene(sceneLoaderOptions.scenePath, sceneLoaderOptions.targetLocation, sceneLoaderOptions.onSceneReady),
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

        async Task LoadScene(string scenePath, Vector3 target, Action<string> onSceneReady)
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

                if (target != Vector3.Zero)
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
            public Action<string> onSceneReady;
        }
    }
}