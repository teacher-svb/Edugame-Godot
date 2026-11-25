using System.Threading.Tasks;
using Godot;
using TnT.EduGame.GameStates;
using TnT.EduGame.Reading;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Waits until a text has been read to the end. With <see cref="_readable"/> set, it waits for the player to read
    /// that thing in the world (its <c>Read</c> signal; closing it early keeps waiting). Otherwise it opens
    /// <see cref="_text"/> in the reading panel itself and waits until the panel is closed (a notice shown in a
    /// scene). Like <see cref="MessageStep"/>, skipped on a listener's catch-up run (no objective).
    /// </summary>
    [GlobalClass]
    public partial class ReadStep : QuestStep
    {
        [Export] Readable _readable;
        [Export] ReadableText _text;

        public override Task Run(QuestObjective objective)
        {
            if (objective == null)
                return Task.CompletedTask;

            var done = new TaskCompletionSource();
            if (_readable != null)
            {
                void OnRead() { _readable.Read -= OnRead; done.TrySetResult(); }
                _readable.Read += OnRead;
            }
            else if (_text != null)
                StateManagerGame.Instance.ShowReading(_text, _ => done.TrySetResult());
            else
            {
                GD.PushWarning($"ReadStep '{Name}': neither a Readable nor a text is set");
                return Task.CompletedTask;
            }
            return done.Task;
        }
    }
}
