using Godot;
using System;
using TnT.EduGame;

public partial class TestBtnSave : Button
{

	public override void _Pressed()
	{
		SaveLoadManager.Instance.SaveGame();
    }
}
