using System;
using Godot;
using TnT.Extensions;
using TnT.Systems;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestTrigger : Node3D
    {
        public enum TriggerType
        {
            INITOBJECTIVE,
            COMPLETEOBJECTIVE
        }
        QuestManager _questManager;

        [Export]
        TriggerType _action;

        [Export]
        string _questId = Guid.Empty.ToString();

        [Export]
        string _questObjectiveId = Guid.Empty.ToString();

        public override void _Ready()
        {
            _questManager = QuestManager.Instance;
            if (_questManager == null)
                GD.Print("QuestManager not found");
        }

        void _OnPlayerTrigger(Node other)
        {
            if (other.FindAnyObjectByType<Player>() == null)
                return;

            _OnTrigger();
        }

        void _OnTrigger()
        {
            switch (_action)
            {
                case TriggerType.INITOBJECTIVE: _questManager.UpdateQuest(new QuestMessageStart { QuestId = _questId, ObjectiveId = _questObjectiveId }); break;
                case TriggerType.COMPLETEOBJECTIVE: _questManager.UpdateQuest(new QuestMessageComplete { QuestId = _questId, ObjectiveId = _questObjectiveId }); break;
            }
        }
    }
}
