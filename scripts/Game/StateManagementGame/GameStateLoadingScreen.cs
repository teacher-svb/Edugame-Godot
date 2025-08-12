using System.Threading.Tasks;
using TnT.Systems.State;
using System;
using Godot;
using TnT.Systems.UI;
using TnT.Extensions;

namespace TnT.EduGame.GameState
{
    public partial class GameStateLoadingScreen : Resource, IStateObject<GameStateLoadingScreen.LoaderOptions>, IGameState
    {
        string _currentScene = "";

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
                if (_currentScene == "")
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
            // ETime[play].timeScale = 0;
            await this.FadeController.ShowView();
        }

        async Task LoadLocation(Player player, Vector2 target)
        {
            //     ETime[play].timeScale = 0;
            await this.FadeController.ShowView();

            //     player.GetComponent<CharacterController2D>().MoveTo(target);
        }

        async Task LoadScene(string sceneName, Player player, Vector2 target, bool forceLoad)
        {
            await FadeIn();

            var gameplayScene = GD.Load<PackedScene>("res://Gameplay/Level1.tscn");
            // var gameplayInstance = gameplayScene.Instantiate();
            // gameplayInstance.Name = "GameplaySceneInstance";
            // ManagerUI.Instance.GetTree().Root.AddChild(gameplayInstance);
            ManagerUI.Instance.GetTree().ChangeSceneToPacked(gameplayScene);

            //     var oldScene = SceneManager.GetSceneByName(_currentScene);
            //     if (oldScene != null && oldScene.IsValid())
            //     {
            //         await SceneManager.UnloadSceneAsync(_currentScene);
            //     }
            //     await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            //     _currentScene = sceneName;

            player.MoveTo(target);
        }

        async Task FadeOut()
        {
            await Task.Delay(1000);

            await this.FadeController.HideView();

            // ETime[play].timeScale = 1;
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
            public string sceneName;
        }
    }
}