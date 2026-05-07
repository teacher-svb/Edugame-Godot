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
        [Export] public InputAction Open { get; private set; }

        public InputActionBase[] InputActions => [Next, Close, Open];

        public override void _EnterTree()
        {
            Instance = this;
        }
    }
}
