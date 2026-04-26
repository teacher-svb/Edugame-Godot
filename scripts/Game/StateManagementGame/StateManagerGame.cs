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

namespace TnT.EduGame.GameState
{
    public partial class StateManagerGame : AbstractStateStack
    {
        public static StateManagerGame Instance { get; private set; }

        public Action<NodePath> OnSceneLoaded;

        Player _player;

        List<BaseGameState> _registeredStates = new();
        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            _player = GetTree().FindAnyObjectByType<Player>();

            var state = _registeredStates.OfType<GameStatePlay>().FirstOrDefault()
                ?? throw new Exception("no play state assigned");
                
            Push(state.GetState(new()));

            OnSceneLoaded?.Invoke(null);
        }

        public void ShowQuestMessage(QuestObjective o)
        {
            ShowMessage(o.GetText(), o.CharacterData);
        }

        public void ShowMessage(string text, CharacterData character)
        {
            var state = _registeredStates.OfType<GameStateMessage>().FirstOrDefault()
                ?? throw new Exception("no dialog state assigned");
            
            Push(state.GetState(new() { text = text, character = character }));
        }

        public void ShowChallenge(IMathChallenge challenge)
        {
            var state = _registeredStates.OfType<GameStateChallenge>().FirstOrDefault()
                ?? throw new Exception("no challenge state assigned");
            
            Push(state.GetState(new() { challenge = challenge }));
        }

        public void LoadScene(string scenePath, Vector3 targetLocation, bool forceLoad = false)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");
                
            Push(state.GetState<SceneLoaderOptions>(new() { scenePath = scenePath, player = _player, targetLocation = targetLocation, forceLoad = forceLoad }));
        }

        public void LoadLocation(Vector3 targetLocation, bool forceLoad = false)
        {
            var state = _registeredStates.OfType<GameStateLoadingScreen>().FirstOrDefault()
                ?? throw new Exception("no scene loader state assigned");
                
            Push(state.GetState<LocationLoaderOptions>(new() { player = _player, targetLocation = targetLocation, forceLoad = forceLoad }));
        }

        internal void RegisterState(BaseGameState gameState)
        {
            _registeredStates.Add(gameState);
        }

        // public void ToggleInventory()
        // {
        //     OpenInventory();
        // }

        // public GameStateInventory OpenInventory(GameStateInventory.OnItemClicked onItemClicked = null)
        // {
        //     GameStateInventory state = _states.OfType<GameStateInventory>().FirstOrDefault();
        //     try
        //     {
        //         ETime[play].timeScale = 0;
        //         Push(state.GetState(new() { }));

        //         return state;
        //     }
        //     catch
        //     {
        //         throw new Exception("no inventory state assigned");
        //     }
        // }
    }
}