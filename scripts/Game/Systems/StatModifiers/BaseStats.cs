using System;
using System.Collections.Generic;
using Godot;

namespace TnT.EduGame
{
    public enum ElementalType
    {
        None,
        Physical,
        Magical
    }
    [GlobalClass]
    public partial class BaseStats : Resource
    {
        [Export]
        public int attack = 10;
        [Export]
        public int defense = 20;
        [Export]
        public int maxHealth = 5;

        // public Dictionary<ElementalType, int> resistances = new()
        // {
        //    {ElementalType.None, 0},
        //    {ElementalType.Magical, 10},
        //    {ElementalType.Physical, -10},
        // };

        [Export]
        public Godot.Collections.Dictionary<ElementalType, int> Resistances { get; set; }
            = new()
        {
           {ElementalType.None, 0},
           {ElementalType.Magical, 10},
           {ElementalType.Physical, -10},
        };
    }

}