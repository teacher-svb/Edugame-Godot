using System.Linq;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestObjectiveReactor : EventListener
    {
        private QuestObjectiveBinding[] _bindings = [];

        public override void _Ready()
        {
            _bindings = GetChildren().OfType<QuestObjectiveBinding>().ToArray();
            base._Ready();
        }

        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is not QuestObjective objective) continue;
                foreach (var binding in _bindings)
                    if (binding.Matches(objective))
                        binding.Execute();
            }
        }
    }
}
