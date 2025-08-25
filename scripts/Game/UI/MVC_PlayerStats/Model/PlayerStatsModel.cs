

using System;
using UnityEngine;

namespace TnT.Systems.PlayerStats
{
    [Serializable]
    public class PlayerStatsModel
    {
        [field: SerializeField]
        public Health PlayerStats { get; set;}
        public float MaxHealth { get => PlayerStats.MaxHealth; }
        public float CurrentHealth { get => PlayerStats.CurrentHealth; set => PlayerStats.CurrentHealth = Mathf.Min(value, MaxHealth); }
        public float CurrentHealthNormalized { get => PlayerStats.CurrentHealth / PlayerStats.MaxHealth; }

    }
}