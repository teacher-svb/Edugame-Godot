using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TnT.Systems.PlayerStats
{
    public class PlayerStatsController : MonoBehaviour
    {
        [SerializeField]
        public PlayerStatsModel model = new PlayerStatsModel();
        [SerializeField]
        public PlayerStatsView view = new PlayerStatsView();

        void Start()
        {
            Initialize();
        }

        async void Initialize()
        {
            await view.InitializeView(model.MaxHealth, 60, 50, 2);

            RefreshView();
        }

        public void Show() {
            view.Show();

            RefreshView();
        }

        public void Hide() {
            view.Hide();
        }

        public void SetHealth(float newCurrentHealth) {
            model.CurrentHealth = newCurrentHealth;

            RefreshView();
        }

        void RefreshView() 
        {
            view.SetHealth(model.CurrentHealthNormalized * 100);
        }
    }
}