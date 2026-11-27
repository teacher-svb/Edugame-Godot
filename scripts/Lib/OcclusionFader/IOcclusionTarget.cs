using Godot;

namespace TnT.Systems.OcclusionFade
{
	/// <summary>
	/// What the <see cref="OcclusionFader"/> keeps visible: it opens a hole in whatever lies between
	/// <see cref="OcclusionCamera"/> and <see cref="OcclusionBody"/>. One per scene (the player).
	/// The fader reads these in its own <c>_Ready</c>, which can run before the target's, so resolve them lazily.
	/// </summary>
	public interface IOcclusionTarget
	{
		Node3D OcclusionBody { get; }
		Camera3D OcclusionCamera { get; }
	}

	/// <summary>Meshes in the subtree of a node implementing this are never faded (e.g. characters).</summary>
	public interface IOcclusionExempt { }
}
