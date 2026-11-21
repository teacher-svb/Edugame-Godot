using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Common.Wfc.Adapters;
using Godoodads.Common.Wfc.Core;
using Godoodads.Dungeon.Core;

namespace Godoodads.Dungeon.Adapters;

/// <summary>
/// The room and corridor tile sets of a <see cref="DungeonSettings"/>, compiled for the solver and
/// checked against brief godoodungeon §4.2, together with the layout and bounds settings.
/// </summary>
public sealed class DungeonTileSets
{
    public TileSetBinding Room { get; private set; }
    public TileSetBinding Corridor { get; private set; }

    /// <summary>The room tile index of <see cref="DungeonSettings.Door"/>, or -1 when it is not in the room tile set.</summary>
    public int DoorTile { get; private set; } = -1;

    /// <summary>The room tile index of <see cref="DungeonSettings.LockedDoor"/>, or -1 when none is chosen.</summary>
    public int LockedDoorTile { get; private set; } = -1;

    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsValid => Errors.Count == 0;

    /// <summary>Checks everything Generate needs before solving. <paramref name="map"/> is optional: with it, its MeshLibrary is checked too.</summary>
    public static DungeonTileSets Check(DungeonSettings settings, GridMap map = null)
    {
        var result = new DungeonTileSets();
        result.Errors.AddRange(settings.CheckBounds());
        result.Errors.AddRange(LayoutValidator.Validate(settings.ToLayoutSettings()));

        if (settings.RoomTileSet == null)
            result.Errors.Add("Choose a Room Tile Set.");
        if (settings.CorridorTileSet == null)
            result.Errors.Add("Choose a Corridor Tile Set.");
        if (settings.RoomTileSet == null || settings.CorridorTileSet == null)
            return result;

        if (!SameLibrary(settings.RoomTileSet.Library, settings.CorridorTileSet.Library))
            result.Errors.Add("The room and corridor tile sets use different MeshLibrary resources. Both must use the GridMap's MeshLibrary.");
        else if (map != null && !SameLibrary(map.MeshLibrary, settings.RoomTileSet.Library))
            result.Errors.Add("The GridMap uses a different MeshLibrary than the tile sets.");

        result.Room = TileSetBinding.Create(settings.RoomTileSet, Directions.All3D);
        result.Corridor = TileSetBinding.Create(settings.CorridorTileSet, Directions.All3D);
        result.Errors.AddRange(result.Room.Errors.Select(e => $"Room tile set: {e}"));
        result.Errors.AddRange(result.Corridor.Errors.Select(e => $"Corridor tile set: {e}"));
        if (!result.Room.IsValid || !result.Corridor.IsValid)
            return result;

        // The door is stored by item (1.0.1); the rules and the solver use the tile's index.
        result.DoorTile = settings.Door == DungeonSettings.NoDoor ? -1 : result.Room.TileForItem(settings.Door);
        var (errors, warnings) = DungeonTileRules.Check(result.Room.Compiled, result.Room.TileNames, result.DoorTile,
            settings.RoomEdgeSocket, result.Corridor.Compiled, settings.PathSocket);
        result.Errors.AddRange(errors);
        result.Warnings.AddRange(warnings);
        CheckLockedDoor(settings, result);
        string edge = settings.RoomEdgeSocket?.Trim() ?? "";
        if (edge.Length > 0 && !(settings.RoomTileSet.ExternalSockets ?? new()).Any(s => s?.Trim() == edge + "-"))
            result.Warnings.Add($"Add '{edge}-' to the room tile set's External Sockets: the room bounds supply it, "
                + $"and without it the tile set preview marks every '{edge}+' face red.");
        return result;
    }

    private static void CheckLockedDoor(DungeonSettings settings, DungeonTileSets result)
    {
        if (settings.LockedDoor == DungeonSettings.NoDoor)
        {
            if (settings.LockedCorridors > 0)
                result.Warnings.Add("Locked Corridors is above 0, but no Locked Door is chosen, so no door is locked.");
            return;
        }
        result.LockedDoorTile = result.Room.TileForItem(settings.LockedDoor);
        if (result.LockedDoorTile < 0)
        {
            result.Errors.Add("The Locked Door is not in the room tile set. Choose it from the room tile set, or (none).");
            return;
        }
        if (result.LockedDoorTile == result.DoorTile)
            result.Warnings.Add("The Locked Door is the Door tile, so locked doors look like every other door.");
        string edge = settings.RoomEdgeSocket?.Trim() ?? "";
        if (edge.Length > 0 && DungeonTileRules.HorizontalDirections.Any(d => DungeonTileRules.DoorVariant(result.Room.Compiled, result.LockedDoorTile, edge, d) < 0))
            result.Errors.Add($"The Locked Door '{result.Room.TileNames[result.LockedDoorTile]}' needs the Door's shape: exactly one horizontal face with '{edge}+' "
                + "(its outside), in four rotations.");
        if (settings.LockedCorridors <= 0)
            result.Warnings.Add("A Locked Door is chosen, but Locked Corridors is 0, so no door is locked.");
    }

    // Two loaded copies of one file count as the same library.
    private static bool SameLibrary(MeshLibrary a, MeshLibrary b) =>
        a != null && b != null && (a == b || (!string.IsNullOrEmpty(a.ResourcePath) && a.ResourcePath == b.ResourcePath));
}
