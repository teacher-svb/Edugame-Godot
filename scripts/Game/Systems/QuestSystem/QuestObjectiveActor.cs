using System;
using Godot;
using TnT.Extensions;

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
            var area = this.FindAncestorOfType<Area2D>();
            area.BodyEntered += OnTriggerEnter2D;
            _questManager = QuestManager.Instance;
            if (_questManager == null)
                GD.Print("QuestManager not found");
        }

        void OnTriggerEnter2D(Node2D other)
        {
            if (other is Player == false)
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
