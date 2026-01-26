using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class CraftLargeDiamondGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.First;
    public override int MaxPlayTime => 0;

    [HideFromEdit]
    private readonly HashSet<Point16> _diamondLocations = [];

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 60 * 16)
        {
            rectangle.Width = 60 * 16;
            modified = true;
        }

        if (rectangle.Height < 30 * 16)
        {
            rectangle.Height = 30 * 16;
            modified = true;
        }

        return modified;
    }

    public override void OnStart()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i <= 20; ++i)
        {
            bool failure = false;
            Point16 pos = DeterminePlacementOfNewDiamond(ref failure);

            if (failure)
            {
                Mod.Logger.Debug("Diamond Dash minigame failed to spawn enough diamonds by default.");
                return;
            }

            PlaceDiamond(pos);
        }
    }

    private Point16 DeterminePlacementOfNewDiamond(ref bool failure)
    {
        int iterations = 3000;

        while (iterations > 0)
        {
            iterations--;
            Point16 position = new(Main.rand.Next(area.X, area.Right) / 16, Main.rand.Next(area.Y, area.Bottom) / 16);

            if (Main.tile[position].HasTile)
                continue;

            return position;
        }

        failure = true;
        return Point16.Zero;
    }

    private void PlaceDiamond(Point16 position)
    {
        WorldGen.PlaceTile(position.X, position.Y, TileID.Diamond, true, false);
        Dust.NewDust(position.ToWorldCoordinates(0, 0), 8, 8, DustID.GemDiamond);
        _diamondLocations.Add(position);

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendTileSquare(-1, position.X, position.Y);
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<AdventurePlayer>().AddPick(TileID.Diamond);
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                [
                    new Item(ItemID.ChlorophytePickaxe),
                ], false);
        }
    }

    public override void ResetPlayer(Player plr)
    {
        plr.GetModPlayer<AdventurePlayer>().RemovePick(TileID.Diamond);
        plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();
    }

    public override void OnStop()
    {
        foreach (var item in _diamondLocations)
        {
            WorldGen.KillTile(item.X, item.Y, false, false, true);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, item.X, item.Y);
        }
    }

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.dead && (plr.HasItem(ItemID.LargeDiamond) || plr.HeldItem.type == ItemID.LargeDiamond))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.dead && (plr.HasItem(ItemID.LargeDiamond) || plr.HeldItem.type == ItemID.LargeDiamond))
            {
                Beaten = true;
                break;
            }    
        }

        if (PlayTime > 10 * 60 && PlayTime % (3 * 60) == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            bool fail = false;
            Point16 pos = DeterminePlacementOfNewDiamond(ref fail);

            if (!fail)
                PlaceDiamond(pos);
        }
    }
}
