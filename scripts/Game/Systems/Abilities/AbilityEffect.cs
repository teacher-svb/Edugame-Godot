using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{
    public enum AbilityOperatorType { Add, Multiply, Subtract }

    public interface IAbilityEffect
    {
        public ElementalType elementalType { get; }
        public AbilityOperatorType operatorType { get; }
        public float value { get; }
        public int duration { get; }
        public int frequency { get; }
        void Apply(Character source, Character target);
    }

}