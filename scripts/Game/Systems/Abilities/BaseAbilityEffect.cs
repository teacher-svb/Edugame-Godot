using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{
    [GlobalClass]
    public abstract partial class BaseAbilityEffect : Resource, IAbilityEffect
    {
        public abstract ElementalType elementalType { get; protected set; }
        public abstract AbilityOperatorType operatorType { get; protected set; }
        public abstract float value { get; protected set; }
        public abstract int duration { get; protected set; }
        public abstract int frequency { get; protected set; }
        public abstract void Apply(Character source, Character target);
    }

}