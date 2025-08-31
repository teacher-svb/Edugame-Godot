using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Easings;
using TnT.Extensions;

public partial class Carrousel : BoxContainer
{
	bool _isScrolling = false;

	[Signal]
	// TODO: When Godot 4.5 releases, consider changing "string" to "Variant"
	public delegate void ValueSelectedEventHandler(string value);
	float[] ratios = { 0, .66f, 1, .66f, 0 };

	public override void _Ready()
	{
		var children = GetChildren().Select(c => c as CarrouselValue).ToArray();
		for (int i = 0; i < children.Length; i++)
			children[i].SizeFlagsStretchRatio = ratios[i];

		HideBorders();
	}

	enum SlideDirection { Previous, Next }

	async Task Slide(SlideDirection direction, float duration = .2f)
	{
		var children = GetChildren().Select(c => c as CarrouselValue).ToArray();
		children.ForEach(c => c.Visible = true);

		var centerIndex = children.Length / 2;

		var centerChild = children[centerIndex];
		var centerChildren = children[(centerIndex - 2)..(centerIndex + 3)]; // range operator is exclusive end

		var childToMove = direction == SlideDirection.Previous ? children[^1] : children[0];
		this.MoveChild(childToMove, direction == SlideDirection.Previous ? 0 : -1);

		var animateFrom = centerChildren.Select(c => c.SizeFlagsStretchRatio).ToArray();
		var animateTo = direction == SlideDirection.Previous ? ratios.Rotate(-1).ToArray() : ratios.Rotate(1).ToArray();

		// Animate
		await foreach (var t in Easings.Animate(duration, Ease.EaseInOutCubic))
		{
			for (int i = 0; i < centerChildren.Length; i++)
			{
				centerChildren[i].SizeFlagsStretchRatio = Mathf.Lerp(animateFrom[i], animateTo[i], t);
				centerChildren[i].Modulate = centerChildren[i].Modulate.Lerp(new Color(animateTo[i], animateTo[i], animateTo[i]), t);
			}
		}


		HideBorders();

		await Task.Yield();

		EmitSignal(SignalName.ValueSelected, 2);
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
		await Slide(SlideDirection.Previous);
		_isScrolling = false;
	}
	public async void _SelectNext()
	{
		if (_isScrolling) return;
		_isScrolling = true;
		await Slide(SlideDirection.Next);
		_isScrolling = false;
	}
}
