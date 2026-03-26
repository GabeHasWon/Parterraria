using Parterraria.Core.BoardSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Parterraria.Core;

internal class AdventurePlayer : ModPlayer
{
    public static bool InAdventure => WorldBoardSystem.PlayingParty || ModContent.GetInstance<ParterrariaServerConfig>().AlwaysAdventureOnServer && Main.netMode != NetmodeID.SinglePlayer;

    private readonly HashSet<int> _allowedPicks = [];
    private readonly HashSet<int> _allowedPlacement = [];

    public override void Load()
    {
        On_Player.PickTile += StopAdventureMining;
        On_Player.PlaceThing += StopAdventurePlacement;
    }

    public void AddPick(params Span<int> ids)
    {
        foreach (int id in ids)
            _allowedPicks.Add(id);
    }

    public void RemovePick(params Span<int> ids)
    {
        foreach (int id in ids)
            _allowedPicks.Remove(id);
    }

    public void AddPick(int id) => _allowedPicks.Add(id);
    public void RemovePick(int id) => _allowedPicks.Remove(id);

    public bool HasPick(int id) => _allowedPicks.Contains(id);

    public void AddPlacement(int id) => _allowedPlacement.Add(id);
    public void RemovePlacement(int id) => _allowedPlacement.Remove(id);

    public bool HasPlacement(int id) => _allowedPlacement.Contains(id);

    private static void StopAdventurePlacement(On_Player.orig_PlaceThing orig, Player self, ref Player.ItemCheckContext context)
    {
        AdventurePlayer advPlr = self.GetModPlayer<AdventurePlayer>();

        if (InAdventure && (!advPlr.HasPlacement(self.HeldItem.type) || ProjectileID.Sets.Explosive[self.HeldItem.shoot]))
            return;

        orig(self, ref context);
    }

    private static void StopAdventureMining(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
    {
        if (InAdventure && !self.GetModPlayer<AdventurePlayer>().HasPick(Main.tile[x, y].TileType))
            return;

        orig(self, x, y, pickPower);
    }
}
