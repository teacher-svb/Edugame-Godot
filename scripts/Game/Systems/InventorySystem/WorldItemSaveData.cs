using System;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// An <see cref="Item"/> placed in a level, keyed by its scene-scoped id: once picked up it stays gone (after a
    /// load, and when the level is entered again).
    /// </summary>
    public class WorldItemSaveData : ISaveable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsNew { get; set; }
        public bool IsPickedUp { get; set; }
    }
}
