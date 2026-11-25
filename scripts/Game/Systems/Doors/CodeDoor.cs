using System;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Platforming;
using TnT.EduGame.Challenges;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Doors
{
    /// <summary>
    /// A closed door that opens with a code: when the player walks into its <see cref="Trigger"/>, it runs its
    /// <see cref="Challenge"/> (a <see cref="MathChallengeTrigger"/>; the key door's is <c>key_code_4.tres</c>, a key to
    /// cut on a <c>KeyVisualization</c>). A <see cref="CodeClue"/> with the trigger as its <c>Source</c> shows the values
    /// (the <c>DungeonPopulator</c> sets the trigger's <c>Seed</c> and the clue's <c>Source</c>). Solved, the leaves
    /// swing into the room (<see cref="OpenAngles"/>) and <see cref="Opened"/> fires; the open state is saved
    /// (<see cref="DoorSaveData"/> under a scene-scoped id, bound by <c>SaveLoadManager</c>), and the trigger keeps the
    /// solved seed (<see cref="MathChallengeTrigger.KeepSolvedSeed"/>), so the clue goes on showing the code that
    /// opened it. Wrong or closed, the player steps out and back in to try again, with the same values.
    /// Stands on a <c>room_door</c> cell in its N-S build (outside to the north, -Z), placed by the game, e.g. the
    /// key door <c>doors/room_door_key.tscn</c>.
    /// </summary>
    [GlobalClass]
    public partial class CodeDoor : Node3D, IBind<DoorSaveData>
    {
        /// <summary>The player entering this area starts the challenge.</summary>
        [Export] public Area3D Trigger { get; set; }

        /// <summary>The code challenge. Override its MathChallenge per door in the inspector (Editable Children).</summary>
        [Export] public MathChallengeTrigger Challenge { get; set; }

        /// <summary>The leaves (bodies with their collision), each turned about Y by its <see cref="OpenAngles"/> entry.</summary>
        [Export] public Node3D[] Leaves { get; set; } = [];

        /// <summary>Degrees about Y per leaf, added to its closed rotation when open. Negative = clockwise seen from above.</summary>
        [Export] public float[] OpenAngles { get; set; } = [-95f, 95f];

        [Export] public float OpenSeconds { get; set; } = 1.2f;

        [Signal] public delegate void OpenedEventHandler();

        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time
        public bool IsOpen { get; private set; }

        DoorSaveData _saveData;
        bool _busy;
        float[] _closedY = [];

        public override void _Ready()
        {
            _closedY = new float[Leaves.Length];
            for (int i = 0; i < Leaves.Length; i++)
                _closedY[i] = Leaves[i].RotationDegrees.Y;
            if (Trigger != null)
                Trigger.BodyEntered += OnBodyEntered;
            // the door opens once: its clue keeps showing the code that opened it
            if (Challenge != null)
                Challenge.KeepSolvedSeed = true;
            if (IsOpen)
                SetOpen(1f);   // bound before _Ready
        }

        public void Bind(DoorSaveData data)
        {
            _saveData = data;
            if (!data.IsNew && data.IsOpen)
            {
                IsOpen = true;
                if (IsNodeReady())
                    SetOpen(1f);
            }
        }

        async void OnBodyEntered(Node3D body)
        {
            if (IsOpen || _busy || Challenge == null || !ChamberSafety.IsPlayer(body))
                return;
            _busy = true;
            try
            {
                // solved, but the level may have been left during the challenge's close: nothing to open then
                if (await Challenge.RunChallengeAsync() && IsInstanceValid(this) && IsInsideTree())
                    await Open();
            }
            catch (Exception e)
            {
                // async void (a signal handler): nothing awaits it, so log here
                GD.PushError($"CodeDoor: opening after the challenge failed: {e}");
            }
            finally
            {
                _busy = false;
            }
        }

        /// <summary>Opens the door (also callable from quest steps), animated, and saves it.</summary>
        public async Task Open()
        {
            if (IsOpen)
                return;
            IsOpen = true;
            if (_saveData != null)
                _saveData.IsOpen = true;
            var tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenMethod(Callable.From<float>(SetOpen), 0f, 1f, OpenSeconds);
            await ToSignal(tween, Tween.SignalName.Finished);
            if (IsInstanceValid(this))
                EmitSignal(SignalName.Opened);
        }

        void SetOpen(float t)
        {
            for (int i = 0; i < Leaves.Length && i < _closedY.Length; i++)
            {
                var r = Leaves[i].RotationDegrees;
                Leaves[i].RotationDegrees = new Vector3(r.X, _closedY[i] + t * (i < OpenAngles.Length ? OpenAngles[i] : 0f), r.Z);
            }
        }
    }
}
