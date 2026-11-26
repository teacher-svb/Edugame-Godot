using System;
using System.Threading.Tasks;
using TnT.Systems.State;
using System.Collections.Generic;
using System.Linq;
using static TnT.EduGame.GameStates.GameStateLoadingScreen;
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.Challenges;
using TnT.Input;

namespace TnT.EduGame.GameStates
{
    public partial class StateManagerGame : AbstractStateStack, IInputActionable
    {
        public static StateManagerGame Instance { get; private set; }

        [Export] public InputAction OpenInventoryAction { get; private set; }
        [Export] public InputAction PauseGameAction { get; private set; }
        [Export] public InputAction PickupItemAction { get; private set; }
        [Export] public InputAction2D MovePlayerAction { get; private set; }
        [Export] public InputAction JumpPlayerAction { get; private set; }

        public InputActionBase[] InputActions => [
            OpenInventoryAction,
            PickupItemAction,
            MovePlayerAction,
            JumpPlayerAction,
            PauseGameAction
        ];

        [Signal] public delegate void SceneLoadedEventHandler(string scenePath);


        List<BaseGameState> _registeredStates = new();

        // the bottom of the stack: the start menu, no gameplay input; never popped
        BaseState _baseState;
        // the play state while it is on the stack (at most one, directly above the base)
        BaseState _playState;
        // the scene or location loader while it is on the stack: states pushed meanwhile go below it
        BaseState _loaderState;
        // the challenge state while it is on the stack
        BaseState _challengeState;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            // the game boots into the base (menu) state, not into Play: pause, inventory and Save don't work at the
            // start menu. A scene loaded through LoadScene gets Play from StartPlay (SaveLoadManager.OnSceneLoaded).
            _baseState = new BaseState(new() { OnEnter = EnterBase, OnResume = EnterBase });
            Push(_baseState);
            // the window's close button quits like the Quit buttons: tree freed and finalizers run before the engine stops
            TnT.Systems.CleanQuit.Install(GetTree());
        }

        // the base is the top: at boot, or after a failed scene change unwound the stack. A level run on its own
        // (a test room started from the editor) or left up by a failed load has a player: it gets the play state.
        Task EnterBase()
        {
            if (GetTree().FindAnyObjectByType<Player>() != null)
                StartPlay();
            return Task.CompletedTask;
        }

        /// <summary>
        /// True while the base (start menu) or the play state is the active top: no menu, dialogue, challenge,
        /// card or load is open and no transition runs.
        /// </summary>
        public bool IsIdle
        {
            get
            {
                var active = ActiveState;
                return active != null && (active == _baseState || active == _playState);
            }
        }

        bool IsLoading => Contains(_loaderState);

        // every push goes through here: while a loader is up, a state pushed (e.g. a quest message from the new
        // level) goes below it, so it shows after the fade-out instead of on the black screen
        void PushState(BaseState state)
        {
            if (IsLoading)
                InsertBelow(_loaderState, state);
            else
                Push(state);
        }

        /// <summary>Whether <paramref name="state"/> is the top of the stack now (active or about to be).</summary>
        public bool IsTop(BaseState state) => state != null && Top == state;

        /// <summary>Pushes a state that is not one of the registered game states (e.g. the dev menu).</summary>
        public void PushOverlay(BaseState state) => PushState(state);

        public void ShowQuestMessage(QuestObjective o, QuestState requestedState, Action onDismissed = null)
        {
            ShowMessage(o.GetText(requestedState), o.CharacterData, onDismissed);
        }

        public void ShowMessage(string text, CharacterData character, Action onDismissed = null)
        {
            var state = _registeredStates.OfType<GameStateMessage>().FirstOrDefault()
                ?? throw new Exception("no dialog state assigned");

            var options = new GameStateMessage.MessageOptions { text = text, character = character, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onDismissed = onDismissed };
            // a box already open (or pushed this frame) takes the message: a second message state on top of it
            // would share the controller's queue and lose its callback
            if (state.IsOpen)
                state.Enqueue(options);
            else
                PushState(state.GetState(options));
        }

        /// <summary>The card between acts (e.g. "Einde van Akte I"); <paramref name="onDismissed"/> runs once it is closed.</summary>
        public void ShowActCard(string title, string subtitle, Action onDismissed = null)
        {
            var state = _registeredStates.OfType<GameStateActCard>().FirstOrDefault()
                ?? throw new Exception("no act card state assigned");

            PushState(state.GetState(new() { title = title, subtitle = subtitle, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onDismissed = onDismissed }));
        }

        /// <summary>
        /// The reading panel for a longer text (letter, notice); <paramref name="onClosed"/> runs once it is closed,
        /// with whether the player reached the last page.
        /// </summary>
        public void ShowReading(Reading.ReadableText text, Action<bool> onClosed = null)
        {
            var state = _registeredStates.OfType<GameStateReading>().FirstOrDefault()
                ?? throw new Exception("no reading state assigned");

            PushState(state.GetState(new() { text = text, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onClosed = onClosed }));
        }

        // seed decides the round's values; pass the same seed to replay a round. False (and nothing shown) while a
        // challenge is already open.
        public bool ShowChallenge(MathChallenge challenge, int? seed = null)
        {
            var state = _registeredStates.OfType<GameStateChallenge>().FirstOrDefault()
                ?? throw new Exception("no challenge state assigned");

            if (Contains(_challengeState))
            {
                GD.PushWarning("StateManagerGame.ShowChallenge: a challenge is already open");
                return false;
            }
            _challengeState = state.GetState(new() { challenge = challenge, seed = seed ?? (int)GD.Randi() });
            PushState(_challengeState);
            return true;
        }

        /// <summary>Closes the open challenge (not whatever is on top); completes once it is hidden.</summary>
        public Task CloseChallenge() => Pop(_challengeState);

        /// <summary>Loads the scene; the player keeps the position from the save data (load game, new game).</summary>
        public void LoadScene(string scenePath) => LoadScene(scenePath, null, null);

        /// <summary>Loads the scene and moves the player to <paramref name="targetLocation"/> (any position, the origin too).</summary>
        public void LoadScene(string scenePath, Vector3 targetLocation) => LoadScene(scenePath, (Vector3?)targetLocation, null);

        /// <summary>
        /// Loads the scene and puts the player at its <see cref="Levels.SpawnPoint"/> at <paramref name="spawnPath"/>
        /// (relative to the scene root; empty = the level's default spawn point), after the save data is bound (so it
        /// wins over the position saved in the previous level).
        /// </summary>
        public void LoadScene(string scenePath, NodePath spawnPath) => LoadScene(scenePath, null, spawnPath ?? new NodePath());

        void LoadScene(string scenePath, Vector3? targetLocation, NodePath spawnPath)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            if (IsLoading)
            {
                GD.PushWarning($"StateManagerGame.LoadScene: already loading, '{scenePath}' ignored");
                return;
            }
            // the loader exits itself once the scene is ready (ExitOnNextUpdate); before changing the scene it
            // unwinds the stack below it to the base state (each state's OnExit runs, its UI hides)
            _loaderState = state.GetState<SceneLoaderOptions>(new()
            {
                scenePath = scenePath,
                targetLocation = targetLocation,
                spawnPath = spawnPath,
                unwind = () => ExitStatesBelow(_loaderState),
                onSceneReady = p => EmitSignal(SignalName.SceneLoaded, p),
            });
            Push(_loaderState);
        }

        public void LoadLocation(Vector3 targetLocation)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            if (IsLoading)
            {
                GD.PushWarning("StateManagerGame.LoadLocation: already loading, ignored");
                return;
            }
            _loaderState = state.GetState<LocationLoaderOptions>(new() { targetLocation = targetLocation });
            Push(_loaderState);
        }

        public void OpenInventory()
        {
            var state = _registeredStates.OfType<GameStateInventory>().FirstOrDefault()
                ?? throw new Exception("no inventory state assigned");

            PushState(state.GetState(new() { close = ManagerUI.Instance.Close }));
        }

        public void OpenPauseMenu()
        {
            var state = _registeredStates.OfType<GameStatePauseMenu>().FirstOrDefault()
                ?? throw new Exception("no pause menu state assigned");

            PushState(state.GetState(new() { close = ManagerUI.Instance.Close }));
        }

        /// <summary>The settings screen with the chosen panels; <paramref name="onClosed"/> runs once it is closed.</summary>
        public void OpenSettings(bool audio = true, bool display = true, bool controls = true, bool accessibility = true, Action onClosed = null)
        {
            var state = _registeredStates.OfType<GameStateSettings>().FirstOrDefault()
                ?? throw new Exception("no settings state assigned");

            PushState(state.GetState(new()
            {
                close            = ManagerUI.Instance.Close,
                showAudio        = audio,
                showDisplay      = display,
                showControls     = controls,
                showAccessibility = accessibility,
                onClosed         = onClosed,
            }));
        }

        /// <summary>
        /// Puts the play state directly above the base state (below a loader still fading out and below messages the
        /// new level pushed during the load). Once per stack: a second call while Play is on it does nothing.
        /// </summary>
        public void StartPlay()
        {
            var state = _registeredStates.OfType<GameStatePlay>().FirstOrDefault()
                ?? throw new Exception("no play state assigned");

            // the player gets its input actions from here (it used to read them from this singleton): the same
            // instances Play enables and disables. Also when Play is already on the stack.
            GetTree().FindAnyObjectByType<Player>()?.SetInputActions(MovePlayerAction, JumpPlayerAction);

            if (Contains(_playState))
                return;
            _playState = state.GetState(new()
            {
                openInventory = OpenInventoryAction,
                pauseGame = PauseGameAction,
                jump = JumpPlayerAction,
                move = MovePlayerAction,
                tutorial = GetTree().FindAnyObjectByType<PlayerTutorial>(),
            });
            Insert(1, _playState);
        }

        internal void RegisterState(BaseGameState gameState)
        {
            _registeredStates.Add(gameState);
        }
    }
}