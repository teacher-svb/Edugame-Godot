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