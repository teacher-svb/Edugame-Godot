using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Systems.Easing;
using TnT.Extensions;

[GlobalClass]
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

	/// <summary>The value in the centre now (ValueSelected only reports changes, the first one inside Init).</summary>
	public string CurrentValue => SelectedValue.Value;

	public void Init()
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

		// Animate (on the tree's process frames, which go on after the carrousel is freed: a challenge closed or its
		// next round shown within a spin; then the rest of the slide is skipped)
		await foreach (var t in Easings.Animate(duration, Ease.EaseInOutCubic))
		{
			if (Gone(children))
				return;
			for (int i = 0; i < children.Length; i++)
			{
				children[i].SizeFlagsStretchRatio = Mathf.Lerp(animateFrom[i], animateTo[i], t);
				children[i].Modulate = children[i].Modulate.Lerp(new Color(animateTo[i], animateTo[i], animateTo[i]), t);
			}
		}
		if (Gone(children))
			return;

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


	bool Gone(CarrouselValue[] children) =>
		!IsInstanceValid(this) || children.Any(c => !IsInstanceValid(c));

	// async void: button handlers (CarrouselController); exceptions are logged here, nothing awaits them
	public void _SelectPrevious() => SlideFromButton(-1);
	public void _SelectNext() => SlideFromButton(1);

	async void SlideFromButton(int direction)
	{
		if (_isScrolling) return;
		_isScrolling = true;
		try
		{
			await Slide(direction);
		}
		catch (Exception e)
		{
			GD.PushError($"Carrousel: slide failed: {e}");
		}
		finally
		{
			_isScrolling = false;
		}
	}
}
