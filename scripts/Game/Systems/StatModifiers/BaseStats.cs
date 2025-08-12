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
    // [CreateAssetMenu(fileName = "BaseStats", menuName = "Stats/BaseStats", order = 0)]
    public partial class BaseStats : Resource
    {
        public int attack = 10;
        public int defense = 20;
        public int maxHealth = 5;

        public Dictionary<ElementalType, int> resistances = new()
        {
           {ElementalType.None, 0},
           {ElementalType.Magical, 10},
           {ElementalType.Physical, -10},
        };
    }

}