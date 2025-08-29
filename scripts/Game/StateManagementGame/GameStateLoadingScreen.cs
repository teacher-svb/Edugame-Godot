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
        Resource _currentScene;

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
            if (options is SceneLoaderOptions)
            {
                var sceneLoaderOptions = options as SceneLoaderOptions;
                if (_currentScene == null)
                    _currentScene = sceneLoaderOptions.sceneName;
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => true,
                    OnEnter = () => LoadScene(sceneLoaderOptions.sceneName, sceneLoaderOptions.player, sceneLoaderOptions.targetLocation, sceneLoaderOptions.forceLoad),
                    OnExit = FadeOut
                });
            }
            else if (options is LocationLoaderOptions)
            {
                var sceneLoaderOptions = options as LocationLoaderOptions;
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => true,
                    OnEnter = () => LoadLocation(sceneLoaderOptions.player, sceneLoaderOptions.targetLocation),
                    OnExit = FadeOut
                });
            }
            else if (options is SceneLoaderOptions)
                return new BaseState(new() { ExitOnNextUpdate = () => true, OnEnter = FadeIn, OnExit = FadeOut });

            return BaseState.GetEmptyState();
        }

        async Task FadeIn()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await this.FadeController.ShowView();
        }

        async Task LoadLocation(Player player, Vector2 target)
        {
            await FadeIn();

            player.MoveTo(target);
        }

        async Task LoadScene(Resource scene, Player player, Vector2 target, bool forceLoad)
        {
            await FadeIn();

            var gameplayScene = GD.Load<PackedScene>(scene.ResourcePath);
            var gameplayInstance = gameplayScene.Instantiate();
            gameplayInstance.Name = "GameplaySceneInstance";

            var tree = ManagerUI.Instance.GetTree();

            tree.Root.ChildEnteredTree += MovePlayer;
            tree.ChangeSceneToPacked(gameplayScene);

            void MovePlayer(Node node)
            {
                var tree = ManagerUI.Instance.GetTree();
                tree.Root.ChildEnteredTree -= MovePlayer;

                var player = ManagerUI.Instance.GetTree().FindAnyObjectByType<Player>();

                player.MoveTo(target);
                GD.Print(target);
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
            public bool forceLoad;
        }

        public class LocationLoaderOptions : LoaderOptions
        {
            public Player player;
            public Vector2 targetLocation;
        }

        public class SceneLoaderOptions : LocationLoaderOptions
        {
            public Resource sceneName;
        }
    }
}