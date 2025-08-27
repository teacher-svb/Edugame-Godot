using System;
using TnT.Systems.State;
using System.Collections.Generic;
using System.Linq;
using static TnT.EduGame.GameState.GameStateLoadingScreen;
using Godot;
using TnT.EduGame.Characters;
using System.Threading.Tasks;
using TnT.Extensions;
using Godot.Collections;
using TnT.EduGame.QuestSystem;

namespace TnT.EduGame.GameState
{
    public abstract partial class GameState : Node
    {
        public override void _Ready()
        {
            var parent = this.FindAncestorOfType<StateManagerGame>();
            parent.RegisterState(this);
        }
    }

    [Serializable]
    public partial class StateManagerGame : AbstractStateStack
    {
        public static StateManagerGame Instance { get; private set; }

        public Action<NodePath> OnSceneLoaded;

        Player _player;

        List<GameState> _states = new();
        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            _player = GetTree().FindAnyObjectByType<Player>();

            GameStatePlay state = _states.OfType<GameStatePlay>().FirstOrDefault();
            try
            {
                Push(state.GetState(new()));
            }
            catch
            {
                throw new Exception("no play state assigned");
            }

            OnSceneLoaded?.Invoke(null);
        }

        public void ShowQuestMessage(QuestObjective o)
        {
            GD.Print(o.GetText());
            ShowMessage(o.GetText(), o.CharacterData);
        }

        public void _on_quest_manager_on_quest_updated()
        {
            GD.Print("test");
        }

        public void ShowMessage(string text, CharacterData character)
        {
            GameStateMessage state = _states.OfType<GameStateMessage>().FirstOrDefault();
            GD.Print(state);
            try
            {
                Push(state.GetState(new() { text = text }));
            }
            catch
            {
                throw new Exception("no dialog state assigned");
            }
        }

        public void LoadScene(Resource sceneName, Vector2 targetLocation, bool forceLoad = false)
        {
            GameStateLoadingScreen state = _states.OfType<GameStateLoadingScreen>().FirstOrDefault();
            try
            {
                Push(state.GetState<SceneLoaderOptions>(new() { sceneName = sceneName, player = _player, targetLocation = targetLocation, forceLoad = forceLoad }));
            }
            catch
            {
                throw new Exception("no scene loader state assigned");
            }
        }

        public void LoadLocation(Vector2 targetLocation, bool forceLoad = false)
        {
            // GetTree().FindAnyObjectByType<Player>();
            GameStateLoadingScreen state = _states.OfType<GameStateLoadingScreen>().FirstOrDefault();
            try
            {
                Push(state.GetState<LocationLoaderOptions>(new() { player = _player, targetLocation = targetLocation, forceLoad = forceLoad }));
            }
            catch
            {
                throw new Exception("no scene loader state assigned");
            }
        }

        internal void RegisterState(GameState gameState)
        {
            _states.Add(gameState);
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