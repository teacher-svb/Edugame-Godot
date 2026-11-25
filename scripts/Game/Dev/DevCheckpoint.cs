using Godot;
using Godot.Collections;
using TnT.EduGame.QuestSystem;

namespace TnT.EduGame.Dev
{
    /// <summary>
    /// A dev jump target: a fresh game where <see cref="CompletedQuests"/> are completed (every objective) and all other
    /// quests are not started, in <see cref="Scene"/> at <see cref="Spawn"/>. The level's quest listeners catch up on
    /// load (exits open, the bridge goes down, ...), as after loading a save. Used by <see cref="DevShortcuts"/>: the
    /// backtick menu or the launch argument <c>--checkpoint=&lt;Id&gt;</c>; the game then saves as
    /// <see cref="DevShortcuts.SaveName"/>, not over the real save.
    /// </summary>
    [GlobalClass]
    public partial class DevCheckpoint : Resource
    {
        /// <summary>Used by the launch argument <c>--checkpoint=&lt;Id&gt;</c>.</summary>
        [Export] public string Id { get; set; } = "";
        /// <summary>Shown in the dev menu (backtick key).</summary>
        [Export] public string Title { get; set; } = "";
        [Export] public Array<Quest> CompletedQuests { get; set; } = [];
        [Export(PropertyHint.File, "*.tscn")] public string Scene { get; set; } = "";
        /// <summary>A spawn point's path in <see cref="Scene"/>; empty = the level's default spawn point.</summary>
        [Export] public string Spawn { get; set; } = "";
        /// <summary>Marks the home tutorial as done, so its prompts don't show.</summary>
        [Export] public bool TutorialDone { get; set; } = true;
    }
}
