using Godot;

namespace TnT.EduGame.QuestSystem
{
    public abstract partial class QuestReaction : Node
    {
        public virtual void Prepare() { }
        public abstract void Execute();
    }
}
