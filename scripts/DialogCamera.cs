using Godot;
using System;
using System.Linq;
using TnT.Extensions;

public partial class DialogCamera : Camera3D
{
	[Export]
	Node3D _visualRoot;
	Camera3D _mainCam;

	public override void _Ready()
	{
		_mainCam = GetTree().FindObjectsByType<Camera3D>().FirstOrDefault(c => c.Current == true);
	}
	
	public void Reset()
	{
		this.Reparent(_visualRoot);
		this.Current = false;
		this._mainCam.Current = true;
	}
}
