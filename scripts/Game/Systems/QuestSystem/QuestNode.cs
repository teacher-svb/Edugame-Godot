using System.Linq;
using Godot;
using TnT.Extensions;

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

		public void StartQuest() => QuestManager.Instance.StartQuest(_quest.Id);

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

		void _OnPlayerStartObjective(Node body, string objectiveId)
		{
			if (IsPlayer(body))
				StartObjective(objectiveId);
		}

		void _OnPlayerCompleteObjective(Node body, string objectiveId)
		{
			if (IsPlayer(body))
				CompleteObjective(objectiveId);
		}

		void _OnPlayerFailObjective(Node body, string objectiveId)
		{
			if (IsPlayer(body))
				FailObjective(objectiveId);
		}

		static bool IsPlayer(Node body) => body.FindAnyObjectByType<Player>() != null;

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
