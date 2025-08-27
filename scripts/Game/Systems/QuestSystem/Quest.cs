using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.Characters;
using TnT.Systems.EventSystem;
using TnT.Systems.Persistence;


namespace TnT.EduGame.QuestSystem
{
    public enum QuestState
    {
        NOTSTARTED,
        INPROGRESS,
        COMPLETED,
        FAILED
    }

    [GlobalClass]
    public partial class Quest : Resource
    {
        [Export]
        public string _id { get; private set; } = Guid.NewGuid().ToString();
        public string Id => _id;

        [Export]
        string _name;
        public string Name => _name;
        [Export]
        Array<QuestObjective> _objectives = new();
        public List<QuestObjective> Objectives => _objectives.ToList();

        public delegate void OnQuestObjectiveStateChanged(QuestObjective objective);
        public OnQuestObjectiveStateChanged OnObjectiveStateChanged;

        [Export]
        public QuestState State
        {
            get => _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED)?.State ?? QuestState.NOTSTARTED;
            set
            {
                var currentObjective = _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED);
                if (currentObjective == null)
                    return;

                currentObjective.State = value;
                GD.Print("emit quest signal");
                OnObjectiveStateChanged.Invoke(currentObjective);
                // QuestManager.Instance.QuestObjectivesChannel.Invoke(currentObjective.ObjectiveId, currentObjective.State);

                if (value == QuestState.COMPLETED)
                {
                    var nextObjective = _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED);
                    if (nextObjective != null)
                        QuestManager.Instance.UpdateQuest(new QuestMessageStart { QuestId = Id, ObjectiveId = nextObjective.ObjectiveId });

                }
            }
        }

        public string CurrentObjectiveId
        {
            get => _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED)?.ObjectiveId ?? Guid.Empty.ToString();
        }

        public void Initialize()
        {
            var firstObjective = _objectives.FirstOrDefault();
            if (firstObjective == null || firstObjective.State == QuestState.COMPLETED)
                return;
            _objectives.FirstOrDefault().State = QuestState.INPROGRESS;
        }

        public void Reset() => Objectives.ForEach(o => o.State = QuestState.NOTSTARTED);

        #region LOAD/SAVE
        private QuestSaveData _saveData;
        public QuestSaveData SaveData => _saveData;

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
        #endregion

    }

    public class QuestSaveData
    {
        public string Id;
        public QuestObjectiveSaveData[] Objectives;
    }

    public class QuestObjectiveSaveData
    {
        public string Id;
        public QuestState State;
    }
}