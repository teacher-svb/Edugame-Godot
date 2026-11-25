using Godot;
using TnT.Systems.Persistence;
using System.Linq;
using TnT.Extensions;

namespace TnT.EduGame.Characters
{
    public partial class Character3D : IBind<CharacterSaveData>
    {
        [Export] public string PersistentId { get; set; }
        /// <summary>
        /// Off (villagers, wizards): the character is saved per level, under "&lt;level scene&gt;#&lt;PersistentId&gt;",
        /// so the same villager in another level (Act I town, Act II town) keeps its own position there.
        /// On (the player, set in Player_3D.tscn): one entry for all levels; LoadScene puts the player at the spawn point.
        /// </summary>
        [Export] public bool SharedAcrossLevels { get; set; }
        CharacterSaveData _saveData;

        // the save key; PersistentId itself stays the plain character id (LoadCharacter)
        string IUnique.PersistentId =>
            SharedAcrossLevels ? PersistentId : $"{GetTree()?.CurrentScene?.SceneFilePath}#{PersistentId}";

        public void Bind(CharacterSaveData data)
        {
            _saveData = data;
            if (!data.IsNew)
                this.Position = _saveData.position;
            // a new entry has no characterId yet: keep this character's own id (was: PersistentId became null)
            LoadCharacter(string.IsNullOrEmpty(_saveData.characterId) ? PersistentId : _saveData.characterId);
        }
    }
}

namespace TnT.EduGame
{
    public partial class Door : IBind<DoorSaveData>
    {
        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time
        DoorSaveData _saveData;

        public void Bind(DoorSaveData data)
        {
            _saveData = data;
            if (!data.IsNew && data.IsOpen)
            {
                _col ??= this.FindAnyObjectByType<CollisionShape3D>(includeSelf: true);
                _animPlayer ??= this.FindAnyObjectByType<AnimationPlayer>(includeSelf: true);
                _col.Disabled = true;
                _animPlayer?.Play("door_open");
                _animPlayer?.Advance(_animPlayer.CurrentAnimationLength);
            }
        }

    }

    public partial class PlayerTutorial : IBind<TutorialSaveData>
    {
        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time
        TutorialSaveData _saveData;

        public void Bind(TutorialSaveData data)
        {
            _saveData = data;
            Visible = !data.IsCompleted;
        }
    }
}

namespace TnT.EduGame.QuestSystem
{
    public partial class QuestManager : IBind<QuestManagerSaveData>
    {
        public string PersistentId { get; private set; } = "QuestManager";
        QuestManagerSaveData _saveData;
        ulong _boundSceneId;   // instance id of the scene the save data was last bound in (0: none)

        /// <summary>
        /// Raised once the loaded scene is bound to the save data: after every node's _Ready and after the other
        /// bindings (SaveLoadManager binds the quests last). Loading raises no quest events, so the listeners catch
        /// up here (<see cref="QuestStepListener.CallWhenSceneReady"/>).
        /// </summary>
        public event System.Action Bound;

        /// <summary>True once <see cref="Bind"/> ran for <paramref name="scene"/> (a node added later can't wait for <see cref="Bound"/>).</summary>
        public bool IsBoundTo(Node scene) => scene != null && _boundSceneId != 0 && scene.GetInstanceId() == _boundSceneId;

        /// <summary>
        /// Sets the quest states in memory to <paramref name="data"/> (null: every quest not started), without quest
        /// events, and drops transitions still pending from the running session. Call it before loading a level for a
        /// loaded save or a new game: the level's listeners read the in-memory states, and <see cref="Bind"/> takes
        /// them as they are (bug-load-stale-quest-states). Until the level binds, nothing is written to the old save data.
        /// </summary>
        public void Restore(QuestManagerSaveData data)
        {
            _saveData = null;   // the running game's save data (a new game must not share or write its objects)
            foreach (var quest in Quests)
            {
                quest.Reset();
                var saved = data?.quests?.FirstOrDefault(s => s.Id == quest.Id);
                if (saved?.Objectives == null)
                    continue;
                foreach (var objective in quest.Objectives)
                {
                    var savedObjective = saved.Objectives.FirstOrDefault(s => s.Id == objective.ObjectiveId);
                    if (savedObjective != null)
                        objective.State = savedObjective.State;
                }
            }
        }

        // The states in memory are the truth here: Restore set them for a load or a new game, and a level change
        // keeps the running session's. Bind attaches the save data and writes them into it.
        public void Bind(QuestManagerSaveData data)
        {
            if (data == null)
            {
                GD.PushError("QuestManager.Bind: no save data");
                return;
            }
            _saveData = data;
            _saveData.Id = PersistentId;
            SyncSaveData();
            _boundSceneId = GetTree()?.CurrentScene?.GetInstanceId() ?? 0;
            Bound?.Invoke();
        }
    }

    // Quest save data: a fresh copy each time (a Resource, so no IBind)
    public partial class Quest
    {
        public QuestSaveData GetSaveData() => new()
        {
            Id = _id,
            Objectives = _objectives.Select(o => o.GetSaveData()).ToArray(),
        };
    }

    public partial class QuestObjective
    {
        public QuestObjectiveSaveData GetSaveData() => new() { Id = _objectiveId, State = State };
    }
}
