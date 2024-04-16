using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class CraftLargeDiamondGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.First;

    private HashSet<Point16> _diamondLocations = [];

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
        for (int i = 0; i <= 20; ++i)
        {
            Point16 position = new(Main.rand.Next(area.X, area.Right) / 16, Main.rand.Next(area.Y, area.Bottom) / 16);

            if (Main.tile[position].HasTile)
            {
                i--;
                continue;
            }

            WorldGen.PlaceTile(position.X, position.Y, TileID.Diamond, true, false);
            Dust.NewDust(position.ToWorldCoordinates(0, 0), 8, 8, DustID.GemDiamond);
            _diamondLocations.Add(position);
        }
    }

    public override void SetupPlayer(Player plr)
    {
        plr.GetModPlayer<AdventurePlayer>().AddPick(TileID.Diamond);
        plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
            [
                new Item(ItemID.ChlorophytePickaxe),
            ]);
    }

    public override void ResetPlayer(Player plr)
    {
        plr.GetModPlayer<AdventurePlayer>().RemovePick(TileID.Diamond);
        plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();
    }

    public override void OnStop()
    {
        foreach (var item in _diamondLocations)
            WorldGen.KillTile(item.X, item.Y, false, false, true);
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
    }
}
