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
        [Export] public InputAction PickupItemAction { get; private set; }
        [Export] public InputAction2D MovePlayerAction { get; private set; }
        [Export] public InputAction JumpPlayerAction { get; private set; }

        public InputActionBase[] InputActions => [OpenInventoryAction, PickupItemAction, MovePlayerAction, JumpPlayerAction];

        [Signal] public delegate void SceneLoadedEventHandler(string scenePath);

        Player _player;

        List<BaseGameState> _registeredStates = new();
        public override void _EnterTree()
        {
            Instance = this;
        }

        public override async void _Ready()
        {
            _player = GetTree().FindAnyObjectByType<Player>();

            var state = _registeredStates.OfType<GameStatePlay>().FirstOrDefault()
                ?? throw new Exception("no play state assigned");

            Push(state.GetState(new() { openInventory = OpenInventoryAction, pickupItem = PickupItemAction }));

            // EmitSignal(SignalName.SceneLoaded, "");
        }

        public void ShowQuestMessage(QuestObjective o)
        {
            ShowMessage(o.GetText(), o.CharacterData);
        }

        public void ShowMessage(string text, CharacterData character)
        {
            var state = _registeredStates.OfType<GameStateMessage>().FirstOrDefault()
                ?? throw new Exception("no dialog state assigned");

            Push(state.GetState(new() { text = text, character = character, next = ManagerUI.Instance.Next, close = ManagerUI.Instance.Close }));
        }

        public void ShowChallenge(IMathChallenge challenge)
        {
            var state = _registeredStates.OfType<GameStateChallenge>().FirstOrDefault()
                ?? throw new Exception("no challenge state assigned");

            Push(state.GetState(new() { challenge = challenge }));
        }

        public void LoadScene(string scenePath, Vector3 targetLocation)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            Push(state.GetState<SceneLoaderOptions>(new() { scenePath = scenePath, targetLocation = targetLocation, onSceneReady = p => EmitSignal(SignalName.SceneLoaded, p) }));
        }

        public void LoadLocation(Vector3 targetLocation)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");

            Push(state.GetState<LocationLoaderOptions>(new() { targetLocation = targetLocation }));
        }

        internal void RegisterState(BaseGameState gameState)
        {
            _registeredStates.Add(gameState);
        }

        public void OpenInventory()
        {
            var state = _registeredStates.OfType<GameStateInventory>().FirstOrDefault()
                ?? throw new Exception("no inventory state assigned");

            Push(state.GetState(new() { close = ManagerUI.Instance.Close }));
        }
    }
}