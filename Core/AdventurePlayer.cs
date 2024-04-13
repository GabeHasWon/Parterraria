using Parterraria.Core.BoardSystem;
using System.Collections.Generic;

namespace Parterraria.Core;

internal class AdventurePlayer : ModPlayer
{
    private readonly HashSet<int> _allowedPicks = [];
    private readonly HashSet<int> _allowedPlacement = [];

    public override void Load()
    {
        On_Player.PickTile += StopPickTileWhenPlaying;
        On_Player.PlaceThing += StopPlacingThingsWhenPlaying;
    }

    public void AddPick(int id)
    {
        if (!_allowedPicks.Contains(id))
            _allowedPicks.Add(id);
    }

    public void RemovePick(int id)
    {
        if (_allowedPicks.Contains(id))
            _allowedPicks.Remove(id);
    }

    public bool HasPick(int id) => _allowedPicks.Contains(id);

    public void AddPlacement(int id)
    {
        if (!_allowedPlacement.Contains(id))
            _allowedPlacement.Add(id);
    }

    public void RemovePlacement(int id)
    {
        if (_allowedPlacement.Contains(id))
            _allowedPlacement.Remove(id);
    }

    public bool HasPlacement(int id) => _allowedPlacement.Contains(id);

    private void StopPlacingThingsWhenPlaying(On_Player.orig_PlaceThing orig, Player self, ref Player.ItemCheckContext context)
    {
        if (WorldBoardSystem.PlayingParty && !self.GetModPlayer<AdventurePlayer>().HasPlacement(self.HeldItem.type))
            return;

        orig(self, ref context);
    }

    private static void StopPickTileWhenPlaying(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
    {
        if (WorldBoardSystem.PlayingParty && !self.GetModPlayer<AdventurePlayer>().HasPick(Main.tile[x, y].TileType))
            return;

        orig(self, x, y, pickPower);
    }
}
