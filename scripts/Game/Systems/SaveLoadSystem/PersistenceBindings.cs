using Godot;
using TnT.Systems.Persistence;
using System.Linq;
using TnT.Extensions;

namespace TnT.EduGame.Characters
{
    public partial class Character3D : IBind<CharacterSaveData>
    {
        [Export] public string PersistentId { get; set; }
        CharacterSaveData _saveData;

        public void Bind(CharacterSaveData data)
        {
            _saveData = data;
            if (!data.IsNew)
                this.Position = _saveData.position;
            LoadCharacter(_saveData.characterId);
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

        public void Bind(QuestManagerSaveData data)
        {
            _saveData = data;
            _saveData.Id = PersistentId;
            Quests.ForEach(q => q.Bind(_saveData.quests?.FirstOrDefault(s => s.Id == q.Id)));
        }
    }
}