
using System;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestEventListener : EventListener
    {
        [Export]
        bool _checkObjectiveId;
        [Export]
        string _objectiveId;
        [Export]
        bool _checkState;
        [Export]
        QuestState _state;
        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective o)
                {
                    if (_checkObjectiveId && o.ObjectiveId != _objectiveId) return;
                    if (_checkState && o.State != _state) return;

                    GD.Print($"Quest {o.ObjectiveId} state changed to {o.State}");

                    base.Raise(o);
                }
            }
        }
    }
}