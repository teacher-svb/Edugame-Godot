using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Easings;
using TnT.Extensions;

public partial class Carrousel : BoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		float[] ratios = new float[] { 0, .66f, 1, .66f, 0 };
		var children = GetChildren();
		for (int i = 0; i < children.Count; i++)
			(children[i] as Control).SizeFlagsStretchRatio = ratios[i];

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	enum SlideDirection { Previous, Next }

	async Task Slide(SlideDirection direction)
	{
		var children = GetChildren();
		children.ForEach(c => (c as Control).Visible = true);

		var currValues = children.Select(c => (c as Control).SizeFlagsStretchRatio).ToArray();

		// Define animation targets based on direction
		float[][] targetRatios = direction == SlideDirection.Previous
			? new float[][] {
			new float[]{ .66f, 1, .66f, 0, 0 },
			new float[]{ 0, .66f, 1, .66f, 0 }
			}
			: new float[][] {
			new float[]{ 0, 0, .66f, 1, .66f },
			new float[]{ 0, .66f, 1, .66f, 0 }
			};

		float[] animateTo = targetRatios[0];
		float[] finalRatios = targetRatios[1];

		// Animate
		await foreach (var t in Easings.Animate(1f, Ease.EaseInOutCubic))
		{
			for (int i = 0; i < children.Count; i++)
				(children[i] as Control).SizeFlagsStretchRatio = Mathf.Lerp(currValues[i], animateTo[i], t);
		}

		// Apply final ratios
		for (int i = 0; i < children.Count; i++)
			(children[i] as Control).SizeFlagsStretchRatio = finalRatios[i];

		// Capture current texts
		var values = children.Select(c => (c.GetChildren()[0] as RichTextLabel).Text).ToArray();

		// Define text remapping based on direction
		int[] mapping = direction == SlideDirection.Previous
			? new[] { 4, 0, 1, 2, 3 }
			: new[] { 1, 2, 3, 4, 0 };

		// Apply remapping
		for (int i = 0; i < children.Count; i++)
			((children[i] as Control).GetChildren()[0] as RichTextLabel).Text = values[mapping[i]];

		// Hide edges
		(children[0] as Control).Visible = false;
		(children[4] as Control).Visible = false;

		await Task.Yield();
	}


	public async void _SelectPrevious()
	{
		await Slide(SlideDirection.Previous);
	}
	public async void _SelectNext()
	{
		await Slide(SlideDirection.Next);
	}
}
