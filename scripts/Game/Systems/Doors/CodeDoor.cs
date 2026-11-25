using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Platforming;
using TnT.EduGame.Challenges;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Doors
{
    /// <summary>
    /// A closed door that opens with a code: when the player walks up to it (<see cref="Trigger"/>), it runs its
    /// <see cref="Challenge"/> (a <see cref="MathChallengeTrigger"/>, usually with a combination lock). Solved, the
    /// leaves swing open and their collision goes; the open state is saved (<see cref="DoorSaveData"/>, bound by
    /// <c>SaveLoadManager</c>). Failed, the player has to step away and come back to try again (same values).
    /// Stands on a <c>room_door</c> cell in its N-S build (outside to the north, -Z), placed by the game, e.g. the
    /// key door <c>doors/room_door_key.tscn</c> with a key challenge.
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

        public string PersistentId { get; private set; }
        public bool IsOpen { get; private set; }

        DoorSaveData _saveData;
        bool _busy;
        float[] _closedY = [];

        public override void _EnterTree() => PersistentId = GetPath().ToString();

        public override void _Ready()
        {
            _closedY = new float[Leaves.Length];
            for (int i = 0; i < Leaves.Length; i++)
                _closedY[i] = Leaves[i].RotationDegrees.Y;
            if (Trigger != null)
                Trigger.BodyEntered += OnBodyEntered;
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
                if (await Challenge.RunChallengeAsync())
                    await Open();
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
