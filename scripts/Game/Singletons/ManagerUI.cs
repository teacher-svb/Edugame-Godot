using Godot;
using System;
using TnT.Input;

namespace TnT.EduGame
{
    public partial class ManagerUI : Node, IInputActionable
    {
        public static ManagerUI Instance { get; private set; }

        [Export] public InputAction Next { get; private set; }
        [Export] public InputAction Close { get; private set; }

        public InputActionBase[] InputActions => [Next, Close];

        public override void _Ready()
        {
            Instance = this;
        }
    }
}
