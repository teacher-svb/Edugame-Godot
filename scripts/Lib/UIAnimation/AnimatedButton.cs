using Godot;

namespace TnT.Systems.UIAnimation
{
    [GlobalClass]
    public partial class AnimatedButton : Button
    {
        [Export] float _maxIntensityWidth = 200f;

        public override void _Ready()
        {
            MouseEntered += async () => await this.HoverBounce(Mathf.Clamp(_maxIntensityWidth / Size.X, 0.5f, 1f));
            MouseExited += async () => await this.UnhoverBounce();
        }
    }
}
