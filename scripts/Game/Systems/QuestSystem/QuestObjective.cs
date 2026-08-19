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
        string _failText;
        public string FailText => _failText;
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
                    // Only ever reached via a failed transition (Quest.Reset() writes State
                    // directly and bypasses GetText), so this doubles as the fail message.
                    icon = $"[color=red]{FailText}[/color]";
                    break;
                case QuestState.INPROGRESS:
                    icon = $"[color=#FFD700][font_size=50]󰲼[/font_size][/color] {StartText}";
                    break;
                case QuestState.COMPLETED:
                    icon = $"[color=green][font_size=50]󰦕[/font_size][/color] {CompleteText}";
                    break;
            }

            return icon;
        }
}
    }