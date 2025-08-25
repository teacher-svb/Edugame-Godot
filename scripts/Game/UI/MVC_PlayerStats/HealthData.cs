
using System;
using UnityEngine;

namespace TnT.Systems.PlayerStats
{
    [CreateAssetMenu(fileName = "New Playerstats", menuName = "PlayerStats")]
    [Serializable]
    public class HealthData : ScriptableObject
    {
        public float MaxHealth;
    }
}