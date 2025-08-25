using System;
using System.Threading.Tasks;
using TnT.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnT.Systems.PlayerStats
{
    [Serializable]
    public class PlayerStatsView
    {
        VisualElement root;
        [SerializeField]
        UIDocument document;
        [SerializeField]
        StyleSheet styleSheet;
        Healthbar healthbar;

        public async Task InitializeView(float maxHealth, int sectionWidth = 1, int sectionHeight = 1, int healthPerSection = 1)
        {
            root = document.rootVisualElement;
            root.Clear();

            root.styleSheets.Add(styleSheet);

            healthbar = root.CreateChild<Healthbar>("healthbar");

            var healthbarWidth = sectionWidth * (maxHealth / healthPerSection);
            healthbar.style.width = new Length
            {
                unit = LengthUnit.Pixel,
                value = healthbarWidth
            };
            healthbar.style.height = new Length
            {
                unit = LengthUnit.Pixel,
                value = sectionHeight
            };

            await Task.Yield();
        }

        public void Show()
        {
            root.visible = true;
        }

        public void Hide()
        {
            root.visible = false;
        }

        public void SetHealth(float fillAmount) => healthbar.SetHealth(fillAmount);
        public void SetHealthbarWidth(float fillAmount) => healthbar.SetHealth(fillAmount);
    }

    public partial class Healthbar : VisualElement
    {
        VisualElement mask;
        VisualElement fill;
        VisualElement border;
        public int Index => parent.IndexOf(this);

        public Healthbar()
        {
            mask = this.CreateChild("mask");
            fill = mask.CreateChild("fill");
            border = this.CreateChild("border");
        }

        public async void SetHealth(float fillAmount)
        {
            var current = fill.style.width.value.value;
            while (current != fillAmount)
            {
                current = Mathf.MoveTowards(current, fillAmount, 1f);
                fill.style.width = new Length
                {
                    unit = LengthUnit.Percent,
                    value = current
                };

                await Task.Yield();
            }
        }
    }
}