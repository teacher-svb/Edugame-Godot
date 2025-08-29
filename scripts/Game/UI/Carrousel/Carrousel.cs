using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Easings;
using TnT.Extensions;

public partial class Carrousel : BoxContainer
{
	bool _isScrolling = false;

	public override void _Ready()
    {
        float[] ratios = [0, .66f, 1, .66f, 0];
        var children = GetChildren().Select(c => c as Control).ToArray();
        for (int i = 0; i < children.Length; i++)
            children[i].SizeFlagsStretchRatio = ratios[i];

		HideBorders();
    }

    enum SlideDirection { Previous, Next }

	async Task Slide(SlideDirection direction, float duration = .2f)
	{
		var children = GetChildren().Select(c => c as Control).ToArray();
        children.ForEach(c => c.Visible = true);

		var centersize = 5;
		var bordersize = (children.Length - centersize) / 2;

		var centerChildren = children.Skip(bordersize).Take(centersize).ToArray();
		var borderChildren = children.Where(c => centerChildren.Contains(c) == false).ToArray();

		var currValues = centerChildren.Select(c => c.SizeFlagsStretchRatio).ToArray();

		var childToMove = direction == SlideDirection.Previous ? children[^1] : children[0];
		this.MoveChild(childToMove, direction == SlideDirection.Previous ? 0 : -1);

		float[] finalRatios = { 0, .66f, 1, .66f, 0 };
		float[] animateTo = direction == SlideDirection.Previous ? finalRatios.Rotate(-1).ToArray() : finalRatios.Rotate(1).ToArray();

		// Animate
		await foreach (var t in Easings.Animate(duration, Ease.EaseInOutCubic))
		{
			for (int i = 0; i < centerChildren.Length; i++)
			{
				centerChildren[i].SizeFlagsStretchRatio = Mathf.Lerp(currValues[i], animateTo[i], t);
				centerChildren[i].Modulate = centerChildren[i].Modulate.Lerp(new Color(animateTo[i], animateTo[i], animateTo[i]), t);
			}
		}

		HideBorders();

		await Task.Yield();
	}

    private void HideBorders()
    {
        var children = GetChildren().Select(c => c as Control).ToArray();
        var centersize = 5;
        var bordersize = (children.Length - centersize) / 2;

        var centerChildren = children.Skip(bordersize).Take(centersize).ToArray();
        var borderChildren = children.Where(c => centerChildren.Contains(c) == false).ToArray();

        centerChildren[0].Visible = false;
        centerChildren[^1].Visible = false;

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
