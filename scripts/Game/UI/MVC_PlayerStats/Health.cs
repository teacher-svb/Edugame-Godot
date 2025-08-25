using TnT.Systems.EventSystem;
using UnityEngine;
using UnityEngine.Events;

namespace TnT.Systems.PlayerStats
{
    public class Health : MonoBehaviour
    {
        public FloatEventChannel PlayerHealthChannel;

        [SerializeField]
        HealthData _staticData;
        [SerializeField]
        private float currentHealth = 0;
        public float CurrentHealth { get => currentHealth; set => currentHealth = value; }
        public float MaxHealth { get => _staticData.MaxHealth; }

        void Awake() {
            CurrentHealth = _staticData.MaxHealth;
        }

        public void Hit(float amount) {
            CurrentHealth -= amount;
            PlayerHealthChannel.Invoke(currentHealth);
        }
    }
}