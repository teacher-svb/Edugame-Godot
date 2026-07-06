using Godot;
using System;
using TnT.EduGame;

public partial class TestBtnLoad : Button
{
	public override void _Pressed()
	{
		SaveLoadManager.Instance.LoadGame("bar");
    }
}
