using System.Linq;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.QuestSystem
{
	/// <summary>
	/// Root of a quest's in-game side. Connect any signal to one of its methods and pass the
	/// objective id as an extra call argument (Connect dialog → Advanced → Add Extra Call Argument).
	/// The _OnPlayer* variants take the body from signals like Area3D.body_entered and only react to the player.
	/// </summary>
	[GlobalClass]
	public partial class QuestNode : Node
	{
		[Export]
		Quest _quest;

		// Don't start the quest here: _Ready runs mid scene load, and pushing a quest message
		// then breaks the loading screen. Start it from a signal (StartQuest) instead.
		public override void _Ready()
		{
			if (_quest == null)
				GD.PrintErr($"QuestNode '{Name}': No quest assigned.");
		}

		public void StartQuest()
		{
			if (_quest == null)
			{
				GD.PrintErr($"QuestNode '{Name}': No quest assigned.");
				return;
			}
			QuestManager.Instance.StartQuest(_quest.Id);
		}

		public void StartObjective(string objectiveId)
		{
			if (IsKnownObjective(objectiveId))
				QuestManager.Instance.StartObjective(_quest.Id, objectiveId);
		}

		public void CompleteObjective(string objectiveId)
		{
			if (IsKnownObjective(objectiveId))
				QuestManager.Instance.CompleteObjective(_quest.Id, objectiveId);
		}

		public void FailObjective(string objectiveId)
		{
			if (IsKnownObjective(objectiveId))
				QuestManager.Instance.FailObjective(_quest.Id, objectiveId);
		}

		/// <summary>The live objective (the quest as <see cref="QuestManager"/> holds it), or null.</summary>
		public QuestObjective FindObjective(string objectiveId)
		{
			if (_quest == null || QuestManager.Instance == null)
				return null;
			var quest = QuestManager.Instance.Quests.FirstOrDefault(q => q.Id == _quest.Id) ?? _quest;
			return quest.Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
		}

		void _OnPlayerStartObjective(Node body, string objectiveId)
		{
			if (Player.IsPlayer(body))
				StartObjective(objectiveId);
		}

		void _OnPlayerCompleteObjective(Node body, string objectiveId)
		{
			if (Player.IsPlayer(body))
				CompleteObjective(objectiveId);
		}

		void _OnPlayerFailObjective(Node body, string objectiveId)
		{
			if (Player.IsPlayer(body))
				FailObjective(objectiveId);
		}

		// A typo in a connection's extra argument would otherwise fail silently.
		bool IsKnownObjective(string objectiveId)
		{
			if (_quest != null && _quest.Objectives.Any(o => o.ObjectiveId == objectiveId))
				return true;

			GD.PrintErr($"QuestNode '{Name}': Quest '{_quest?.Name}' has no objective '{objectiveId}'.");
			return false;
		}
	}
}
