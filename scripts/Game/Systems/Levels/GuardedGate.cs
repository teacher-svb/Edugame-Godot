using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.GameStates;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// A way that guards keep closed (card act1-mana-spring-guards: the Mana Spring in the forest): its
    /// <see cref="Guards"/> stand in front of a <see cref="Block"/> that stops the player, and a guard barks
    /// <see cref="BarkText"/> when the player walks into <see cref="BarkArea"/> (again after stepping out).
    /// <see cref="Unguard"/> removes guards, block and bark for good, e.g. from a <c>CallMethodStep</c> under a
    /// <c>QuestsCompletedListener</c> (IV.2), which also catches up after loading, so the gate needs no save data.
    /// </summary>
    [GlobalClass]
    public partial class GuardedGate : Node3D
    {
        [Export] public Godot.Collections.Array<Node3D> Guards { get; set; } = new();

        /// <summary>What stops the player, e.g. a StaticBody3D barrier.</summary>
        [Export] public Node3D Block { get; set; }

        [Export] public Area3D BarkArea { get; set; }
        [Export(PropertyHint.MultilineText)] public string BarkText { get; set; } = "";
        [Export] public CharacterData BarkCharacter { get; set; }

        public bool IsGuarded { get; private set; } = true;

        /// <summary>How often a guard barked in this level.</summary>
        public int Barks { get; private set; }

        // physics frames after the level started in which an entering player doesn't count (arrived inside)
        const int SettleFrames = 5;
        int _frames;
        bool _inside;

        public override void _Ready()
        {
            if (BarkArea == null)
                return;
            BarkArea.BodyEntered += OnBarkEntered;
            BarkArea.BodyExited += OnBarkExited;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_frames < SettleFrames)
                _frames++;
        }

        /// <summary>The guards leave: guards, block and bark area are removed. Safe to call again.</summary>
        public void Unguard()
        {
            if (!IsGuarded)
                return;
            IsGuarded = false;
            foreach (var guard in Guards)
                guard?.QueueFree();
            Block?.QueueFree();
            BarkArea?.QueueFree();
            Guards.Clear();
            Block = null;
            BarkArea = null;
        }

        void OnBarkEntered(Node3D body)
        {
            if (!Player.IsPlayer(body) || _inside)
                return;
            _inside = true;
            if (!IsGuarded || _frames < SettleFrames || string.IsNullOrWhiteSpace(BarkText) || BarkCharacter == null)
                return;
            Barks++;
            StateManagerGame.Instance.ShowMessage(BarkText, BarkCharacter);
        }

        void OnBarkExited(Node3D body)
        {
            if (Player.IsPlayer(body))
                _inside = false;
        }
    }
}
