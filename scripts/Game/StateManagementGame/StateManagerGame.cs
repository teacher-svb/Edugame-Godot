using System;
using TnT.Systems.State;
using System.Collections.Generic;
using System.Linq;
using static TnT.EduGame.GameState.GameStateLoadingScreen;
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.Question;
using TnT.Input;

namespace TnT.EduGame.GameState
{
    public partial class StateManagerGame : AbstractStateStack, IInputActionable
    {
        public static StateManagerGame Instance { get; private set; }

        [Export] public InputAction OpenInventoryAction { get; private set; }
        [Export] public InputAction PauseGameAction { get; private set; }
        [Export] public InputAction PickupItemAction { get; private set; }
        [Export] public InputAction2D MovePlayerAction { get; private set; }
        [Export] public InputAction JumpPlayerAction { get; private set; }
        Player _player;
        PlayerTutorial _tutorial;

        public InputActionBase[] InputActions => [
            OpenInventoryAction,
            PickupItemAction,
            MovePlayerAction,
            JumpPlayerAction,
            PauseGameAction
        ];

        [Signal] public delegate void SceneLoadedEventHandler(string scenePath);


        List<BaseGameState> _registeredStates = new();
        public override void _EnterTree()
        {
            Instance = this;
        }

        public override async void _Ready()
        {
            _player = GetTree().FindAnyObjectByType<Player>();
            _tutorial = GetTree().FindAnyObjectByType<PlayerTutorial>();

            var state = _registeredStates.OfType<GameStatePlay>().FirstOrDefault()
                ?? throw new Exception("no play state assigned");

            Push(state.GetState(
                new() {
                    openInventory = OpenInventoryAction,
                    pauseGame = PauseGameAction,
                    pickupItem = PickupItemAction,
                    jump = JumpPlayerAction,
                    move = MovePlayerAction,
                    tutorial = _tutorial
                }));
        }

        public void ShowQuestMessage(QuestObjective o, QuestState requestedState, Action onDismissed = null)
        {
            ShowMessage(o.GetText(requestedState), o.CharacterData, onDismissed);
        }

        public void ShowMessage(string text, CharacterData character, Action onDismissed = null)
        {
            var state = _registeredStates.OfType<GameStateMessage>().FirstOrDefault()
                ?? throw new Exception("no dialog state assigned");

            Push(state.GetState(new() { text = text, character = character, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onDismissed = onDismissed }));
        }

        /// <summary>The card between acts (e.g. "Einde van Akte I"); <paramref name="onDismissed"/> runs once it is closed.</summary>
        public void ShowActCard(string title, string subtitle, Action onDismissed = null)
        {
            var state = _registeredStates.OfType<GameStateActCard>().FirstOrDefault()
                ?? throw new Exception("no act card state assigned");

            Push(state.GetState(new() { title = title, subtitle = subtitle, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onDismissed = onDismissed }));
        }

        /// <summary>
        /// The reading panel for a longer text (letter, notice); <paramref name="onClosed"/> runs once it is closed,
        /// with whether the player reached the last page.
        /// </summary>
        public void ShowReading(Reading.ReadableText text, Action<bool> onClosed = null)
        {
            var state = _registeredStates.OfType<GameStateReading>().FirstOrDefault()
                ?? throw new Exception("no reading state assigned");

            Push(state.GetState(new() { text = text, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close, onClosed = onClosed }));
        }

        // seed decides the round's values; pass the same seed to replay a round
        public void ShowChallenge(MathChallenge challenge, int? seed = null)
        {
            var state = _registeredStates.OfType<GameStateChallenge>().FirstOrDefault()
                ?? throw new Exception("no challenge state assigned");

            Push(state.GetState(new() { challenge = challenge, seed = seed ?? (int)GD.Randi() }));
        }

        public void LoadScene(string scenePath, Vector3 targetLocation) => LoadScene(scenePath, targetLocation, null);

        /// <summary>
        /// Loads the scene and puts the player at its <see cref="Levels.SpawnPoint"/> at <paramref name="spawnPath"/>
        /// (relative to the scene root; empty = the level's default spawn point), after the save data is bound (so it
        /// wins over the position saved in the previous level).
        /// </summary>
        public void LoadScene(string scenePath, NodePath spawnPath) => LoadScene(scenePath, Vector3.Zero, spawnPath ?? new NodePath());

        void LoadScene(string scenePath, Vector3 targetLocation, NodePath spawnPath)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            // ResetStack();
            Push(state.GetState<SceneLoaderOptions>(new() { scenePath = scenePath, targetLocation = targetLocation, spawnPath = spawnPath, onSceneReady = p => { Pop(); EmitSignal(SignalName.SceneLoaded, p); } }));
        }

        public void LoadLocation(Vector3 targetLocation)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            Push(state.GetState<LocationLoaderOptions>(new() { targetLocation = targetLocation }));
        }

        public void OpenInventory()
        {
            var state = _registeredStates.OfType<GameStateInventory>().FirstOrDefault()
                ?? throw new Exception("no inventory state assigned");

            Push(state.GetState(new() { close = ManagerUI.Instance.Close }));
        }

        public void OpenPauseMenu()
        {
            var state = _registeredStates.OfType<GameStatePauseMenu>().FirstOrDefault()
                ?? throw new Exception("no pause menu state assigned");

            Push(state.GetState(new() { close = ManagerUI.Instance.Close }));
        }

        public void OpenSettings(bool audio = true, bool display = true, bool controls = true, bool accessibility = true)
        {
            var state = _registeredStates.OfType<GameStateSettings>().FirstOrDefault()
                ?? throw new Exception("no settings state assigned");

            Push(state.GetState(new()
            {
                close            = ManagerUI.Instance.Close,
                showAudio        = audio,
                showDisplay      = display,
                showControls     = controls,
                showAccessibility = accessibility,
            }));
        }

        public void StartPlay()
        {
            _player = GetTree().FindAnyObjectByType<Player>();
            _tutorial = GetTree().FindAnyObjectByType<PlayerTutorial>();

            var state = _registeredStates.OfType<GameStatePlay>().FirstOrDefault()
                ?? throw new Exception("no play state assigned");

            Push(state.GetState(new() { openInventory = OpenInventoryAction, pickupItem = PickupItemAction, pauseGame = PauseGameAction, tutorial = _tutorial }));
        }

        internal void RegisterState(BaseGameState gameState)
        {
            _registeredStates.Add(gameState);
        }
    }
}