namespace TnT.Systems.Persistence
{
    /// <summary>
    /// Root of a save file. A plain C# class (not a Godot Resource), so Newtonsoft only writes the game's own
    /// fields; save data classes must stay plain C# too (no Godot object fields).
    /// </summary>
    public abstract class GameData
    {
        public string Name;
    }
}
