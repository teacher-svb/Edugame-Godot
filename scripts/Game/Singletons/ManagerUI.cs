using Godot;
using System;

namespace TnT.EduGame
{
    public partial class ManagerUI : Control
    {
        public static ManagerUI Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }
    }
}
