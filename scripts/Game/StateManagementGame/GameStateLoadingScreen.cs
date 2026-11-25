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
        FadeController _fadeController;
        FadeController FadeController
        {
            get
            {
                if (_fadeController == null)
                    _fadeController = ManagerUI.Instance.FindAnyObjectByType<FadeController>(includeSelf: true);
                return _fadeController;
            }
        }

        // the black screen stays this long after the load before it fades out
        const double FadeOutDelay = 1.0;

        public BaseState GetState(LoaderOptions options)
        {
            return GetState<LoaderOptions>(options);
        }

        /// <summary>
        /// A loader state: fades to black, loads, and exits itself once done (also when the load failed: the error is
        /// logged, the old scene stays); its OnExit fades back in and unpauses.
        /// </summary>
        public BaseState GetState<T>(T options) where T : ISceneLoaderOptions, new()
        {
            // per push, not per node: a loader's flag must not be reset or set by another one
            bool finished = false;
            void Finish() => finished = true;

            if (options is SceneLoaderOptions sceneLoaderOptions)
            {
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => finished,
                    OnEnter = () => LoadScene(sceneLoaderOptions, Finish),
                    OnExit = FadeOut
                });
            }
            else if (options is LocationLoaderOptions locationLoaderOptions)
            {
                return new BaseState(new()
                {
                    ExitOnNextUpdate = () => finished,
                    OnEnter = () => LoadLocation(locationLoaderOptions.targetLocation, Finish),
                    OnExit = FadeOut
                });
            }

            // nothing to load: a state that exits right away instead of one that never does
            GD.PushWarning($"GameStateLoadingScreen: no loader for {options?.GetType().Name ?? "null"} options");
            return new BaseState(new() { ExitOnNextUpdate = () => true });
        }

        // the managers were freed during a fade (the game quits, CleanQuit): no scene to change any more
        bool Gone => !IsInstanceValid(this) || !IsInsideTree();

        async Task FadeIn()
        {
            var tree = ManagerUI.Instance.GetTree();
            tree.Paused = true;
            await this.FadeController.ShowView();
        }

        async Task LoadLocation(Vector3 target, Action finish)
        {
            try
            {
                await FadeIn();
                if (Gone)
                    return;

                var tree = ManagerUI.Instance.GetTree();
                var player = tree.FindAnyObjectByType<Player>();
                if (player == null)
                    GD.PushError("GameStateLoadingScreen.LoadLocation: no player to move");
                else
                    player.MoveTo(target);
            }
            catch (Exception e)
            {
                GD.PushError($"GameStateLoadingScreen.LoadLocation failed: {e}");
            }
            finally
            {
                finish();
            }
        }

        async Task LoadScene(SceneLoaderOptions options, Action finish)
        {
            var scenePath = options.scenePath;
            var tree = ManagerUI.Instance.GetTree();
            bool subscribed = false;
            try
            {
                await FadeIn();
                if (Gone)
                    return;

                // checked before anything is torn down: a failed load leaves the old scene and its states as they were
                var gameplayScene = LoadPackedScene(scenePath);
                if (gameplayScene == null)
                {
                    finish();
                    return;
                }

                // unwind the stack below this loader to the base state (each OnExit runs, the UI hides behind the
                // black fade); an exited state may have unpaused the tree
                if (options.unwind != null)
                    await options.unwind();
                if (Gone)
                    return;
                tree.Paused = true;

                tree.Root.ChildEnteredTree += MovePlayer;
                subscribed = true;
                var error = tree.ChangeSceneToPacked(gameplayScene);
                if (error != Error.Ok)
                {
                    tree.Root.ChildEnteredTree -= MovePlayer;
                    GD.PushError($"GameStateLoadingScreen: changing to '{scenePath}' failed: {error}");
                    finish();
                }
            }
            catch (Exception e)
            {
                if (subscribed)
                    tree.Root.ChildEnteredTree -= MovePlayer;
                GD.PushError($"GameStateLoadingScreen: loading '{scenePath}' failed: {e}");
                finish();
            }

            async void MovePlayer(Node node)
            {
                tree.Root.ChildEnteredTree -= MovePlayer;
                try
                {
                    await node.ToSignal(node, Node.SignalName.Ready);
                    if (Gone)
                        return;   // quit while the scene was switching: the managers are freed

                    options.onSceneReady?.Invoke(scenePath);

                    // after binding: a saved position (from the previous level) must not win over the spawn point
                    if (options.spawnPath != null)
                    {
                        var spawn = Levels.SpawnPoint.Find(tree, options.spawnPath, out bool missing);
                        if (missing)
                            GD.PushWarning($"{scenePath}: no spawn point at '{options.spawnPath}', using the default one");
                        if (spawn != null)
                            tree.FindAnyObjectByType<Player>()?.MoveTo(spawn.GlobalPosition);
                        else
                            GD.PushWarning($"{scenePath}: no default spawn point");
                    }
                    else if (options.targetLocation is Vector3 target)
                        tree.FindAnyObjectByType<Player>()?.MoveTo(target);
                }
                catch (Exception e)
                {
                    GD.PushError($"GameStateLoadingScreen: setting up '{scenePath}' failed: {e}");
                }
                finally
                {
                    finish();
                }
            }
        }

        // null (with an error) for an unknown path or uid://, or a resource that is no scene
        static PackedScene LoadPackedScene(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                GD.PushError("GameStateLoadingScreen: no scene path given");
                return null;
            }
            Resource resource = null;
            try
            {
                resource = ResourceLoader.Load(scenePath);
            }
            catch (Exception e)
            {
                GD.PushError($"GameStateLoadingScreen: loading '{scenePath}' threw: {e}");
            }
            if (resource is PackedScene scene)
                return scene;
            GD.PushError($"GameStateLoadingScreen: no scene at '{scenePath}' (use a full res:// path; a new uid:// is " +
                "only known after the editor scanned the project)");
            return null;
        }

        async Task FadeOut()
        {
            var tree = ManagerUI.Instance.GetTree();
            // a SceneTreeTimer that runs while paused (the tree is paused during the load), not a wall-clock delay
            await ToSignal(tree.CreateTimer(FadeOutDelay, processAlways: true), SceneTreeTimer.SignalName.Timeout);

            await this.FadeController.HideView();

            tree.Paused = false;
        }

        public interface ISceneLoaderOptions { }

        public class LoaderOptions : ISceneLoaderOptions { }

        public class LocationLoaderOptions : LoaderOptions
        {
            public Vector3 targetLocation;
        }

        public class SceneLoaderOptions : LoaderOptions
        {
            public string scenePath;
            /// <summary>Where to put the player once the scene is ready; null: no target (a loaded save keeps its position).</summary>
            public Vector3? targetLocation;
            /// <summary>
            /// The spawn point's node path relative to the scene root. null: no spawn point (a loaded save keeps its
            /// position); empty: the level's default one. Wins over <see cref="targetLocation"/>.
            /// </summary>
            public NodePath spawnPath;
            /// <summary>Exits the states below the loader, after the fade-in and before the scene changes.</summary>
            public Func<Task> unwind;
            public Action<string> onSceneReady;
        }
    }
}