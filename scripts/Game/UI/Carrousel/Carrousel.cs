using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Easings;
using TnT.Extensions;

public partial class Carrousel : BoxContainer
{
	[Export] int startIndex = 0;

	bool _isScrolling = false;

	[Signal]
	// TODO: When Godot 4.5 releases, consider changing "string" to "Variant"
	public delegate void ValueSelectedEventHandler(string value);
	float[] ratios = { 0, .66f, 1, .66f, 0 };
	CarrouselValue[] Children { get => GetChildren().Select(c => c as CarrouselValue).ToArray(); }
	float[] AllRatios { get => ratios.Pad(Children.Length).ToArray(); }

	CarrouselValue SelectedValue => Children[Children.Length / 2];

	public override void _Ready()
	{
		var numChildren = GetChildren().Count;
		var centerIndex = numChildren / 2;

		_ = Slide((startIndex % numChildren) - centerIndex, 0);
	}

	async Task Slide(int direction, float duration = .2f)
	{
		var children = Children;
		var allRatios = AllRatios;

		children.ForEach(c => c.Visible = true);

		if (direction != 0)
		{
			for (int i = 0; i < Mathf.Abs(direction); ++i)
			{
				var childToMove = direction < 0 ? children[^(1 + i)] : children[i];
				this.MoveChild(childToMove, direction < 0 ? 0 : -1);
			}
		}

		var animateFrom = children.Select(c => c.SizeFlagsStretchRatio).ToArray();
		var animateTo = allRatios.Rotate(direction).ToArray();

		// Animate
		await foreach (var t in Easings.Animate(duration, Ease.EaseInOutCubic))
		{
			for (int i = 0; i < children.Length; i++)
			{
				children[i].SizeFlagsStretchRatio = Mathf.Lerp(animateFrom[i], animateTo[i], t);
				children[i].Modulate = children[i].Modulate.Lerp(new Color(animateTo[i], animateTo[i], animateTo[i]), t);
			}
		}

		HideBorders();

		EmitSignal(SignalName.ValueSelected, SelectedValue.Value);
	}

	private void HideBorders()
	{
		var children = GetChildren().Select(c => c as CarrouselValue).ToArray();
		var centerIndex = children.Length / 2;

		var borderChildren = children
			.Where((c, i) => i < centerIndex - 1 || i > centerIndex + 1)
			.ToArray();

		borderChildren.ForEach(c => c.Visible = false);
	}


	public async void _SelectPrevious()
	{
		if (_isScrolling) return;
		_isScrolling = true;
		await Slide(-1);
		_isScrolling = false;
	}
	public async void _SelectNext()
	{
		if (_isScrolling) return;
		_isScrolling = true;
		await Slide(1);
		_isScrolling = false;
	}
}
