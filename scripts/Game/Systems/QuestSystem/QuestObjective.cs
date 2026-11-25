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

        // the state requested and waiting for its quest message to be dismissed (Quest.RequestTransition); not saved
        internal QuestState? PendingState { get; set; }
        public bool HasPendingTransition => PendingState != null;

        public string GetText()
        {
            return GetText(State);
        }

        /// <summary>The quest message for a move to <paramref name="state"/>; empty when there is nothing to say.</summary>
        public string GetText(QuestState state)
        {
            string icon = "";

            switch (state)
            {
                case QuestState.NOTSTARTED:
                    // Only ever reached via a failed transition (Quest.Reset() writes State
                    // directly and bypasses GetText), so this doubles as the fail message.
                    // No fail text: no message (was an empty red box).
                    if (!string.IsNullOrWhiteSpace(FailText))
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
