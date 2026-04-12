using System;
using Godot;
using TnT.Extensions;
using TnT.Systems;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestObjectiveActor : Node
    {
        public enum QuestObjectiveAction
        {
            INITOBJECTIVE,
            COMPLETEOBJECTIVE
        }
        QuestManager _questManager;

        [Export]
        QuestObjectiveAction _action;

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

        void _OnBodyEntered(Node other)
        {
            if (other.FindAnyObjectByType<Player>() == null)
                return;

            GD.Print("entered quest starter");

            switch (_action)
            {
                case QuestObjectiveAction.INITOBJECTIVE: _questManager.UpdateQuest(new QuestMessageStart { QuestId = _questId, ObjectiveId = _questObjectiveId }); break;
                case QuestObjectiveAction.COMPLETEOBJECTIVE: _questManager.UpdateQuest(new QuestMessageComplete { QuestId = _questId, ObjectiveId = _questObjectiveId }); break;
            }
        }
    }
}
