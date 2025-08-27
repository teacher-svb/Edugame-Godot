using System;
using Godot;
using TnT.EduGame.Characters;



namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestObjective : Resource
    {
        [Export]
        string _startText;
        public string StartText => _startText;
        [Export]
        string _progressText;
        public string ProgressText => _progressText;
        [Export]
        string _completeText;
        [Export]
        CharacterData _characterData;
        public CharacterData CharacterData => _characterData;
        public string CompleteText => _completeText;

        [Export]
        string _objectiveId = Guid.NewGuid().ToString();
        public string ObjectiveId => _objectiveId;
        [Export]
        public QuestState State;

        #region LOAD/SAVE
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
        #endregion

        public string GetText()
        {
            return GetText(State);
        }

        public string GetText(QuestState state)
        {
            string icon = "";

            switch (state)
            {
                case QuestState.NOTSTARTED:
                    icon = "";
                    break;
                case QuestState.INPROGRESS:
                    icon = $"<color=#FFD700><size=50>󰲼</size> Start:</color> {StartText}";
                    break;
                case QuestState.COMPLETED:
                    icon = $"<color=green><size=50>󰦕</size> Klaar!</color> {CompleteText}";
                    break;
            }

            return icon;
        }
}
    }