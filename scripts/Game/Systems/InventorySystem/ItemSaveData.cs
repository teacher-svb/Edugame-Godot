namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// One filled inventory slot in the save file. The item is stored by reference to its <see cref="ItemData"/>
    /// resource file (a <c>uid://</c> when it has one, else its <c>res://</c> path), so the item must be a saved
    /// .tres, not a sub-resource embedded in a scene.
    /// </summary>
    public class ItemSaveData
    {
        public int Slot;
        public string ItemId;
        public string Resource;
    }
}
