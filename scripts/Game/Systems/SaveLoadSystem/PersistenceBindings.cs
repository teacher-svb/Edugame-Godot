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
        public string PersistentId { get; private set; }
        DoorSaveData _saveData;

        public void Bind(DoorSaveData data)
        {
            GD.Print($"door bind {data} {data?.IsOpen}");
            _saveData = data;
            if (!data.IsNew && data.IsOpen)
            {
                _col ??= this.FindAnyObjectByType<CollisionShape3D>();
                _animPlayer ??= this.FindAnyObjectByType<AnimationPlayer>();
                _col.Disabled = true;
                _animPlayer?.Play("door_open");
                _animPlayer?.Advance(_animPlayer.CurrentAnimationLength);
            }
        }

    }

    public partial class PlayerTutorial : IBind<TutorialSaveData>
    {
        public string PersistentId { get; private set; }
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

        /// <summary>
        /// Raised after the quest states are restored from the save data. That happens after the level's _Ready
        /// (the scene loader binds once the scene is in the tree) and raises no quest events, so anything that
        /// reads quest states on _Ready (e.g. <see cref="QuestsCompletedListener"/>) checks again here.
        /// </summary>
        public event System.Action Bound;

        public void Bind(QuestManagerSaveData data)
        {
            _saveData = data;
            _saveData.Id = PersistentId;
            Quests.ForEach(q => q.Bind(_saveData.quests?.FirstOrDefault(s => s.Id == q.Id)));
            Bound?.Invoke();
        }
    }

    // Quest class binding implementation
    // Note: This one does NOT inherit from IBind<T> because it is a Resource and not a Node
    public partial class Quest
    {
        QuestSaveData _saveData;

        public void Bind(QuestSaveData data)
        {
            if (data == null)
            {
                Reset();
                return;
            }
            _saveData = data;
            _id = _saveData.Id;
            Objectives.ForEach(o => o.Bind(data.Objectives.FirstOrDefault(s => s.Id == o.ObjectiveId)));
        }

        public QuestSaveData GetSaveData()
        {
            if (_saveData == null)
                _saveData = new();
            _saveData.Id = _id;
            _saveData.Objectives = _objectives.Select(o => o.GetSaveData()).ToArray();
            return _saveData;
        }
    }

    // QuestObjective class binding implementation
    // Note: This one does NOT inherit from IBind<T> because it is a Resource and not a Node
    public partial class QuestObjective
    {
        QuestObjectiveSaveData _saveData;
        public QuestObjectiveSaveData SaveData => _saveData;
        public void Bind(QuestObjectiveSaveData data)
        {
            if (data == null)
                return;
            _saveData = data;
            State = _saveData.State;
            _objectiveId = _saveData.Id;
        }

        public QuestObjectiveSaveData GetSaveData()
        {
            if (_saveData == null)
                _saveData = new();
            _saveData.Id = _objectiveId;
            _saveData.State = State;
            return _saveData;
        }
    }
}