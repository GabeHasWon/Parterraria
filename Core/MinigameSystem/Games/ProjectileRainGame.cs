using Parterraria.Common.TileCommon;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class ProjectileRainGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.Last;

    public int MaxTime = 0;

    [HideFromEdit]
    private int _timer = 0;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 40 * 16)
        {
            rectangle.Width = 30 * 16;
            modified = true;
        }

        if (rectangle.Height < 30 * 16)
        {
            rectangle.Height = 25 * 16;
            modified = true;
        }

        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                [
                    new Item(ItemID.EoCShield),
                ], false);
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.dead && (plr.HasItem(ItemID.GrubSoup) || plr.HeldItem.type == ItemID.GrubSoup))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        Point16 topLeft = area.TopLeft().ToTileCoordinates16();
        Point16 size = area.Size().ToTileCoordinates16();

        for (int x = topLeft.X; x < topLeft.X + size.X; ++x)
        {
            for (int y = topLeft.Y; y < topLeft.Y + size.Y; ++y)
            {
                Tile tile = Main.tile[x, y];

                if (tile.HasTile && tile.TileType == TileID.JungleGrass && Main.rand.NextBool(5))
                    RandomUpdating.Auto(x, y, false, 0);
            }
        }

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.dead && (plr.HasItem(ItemID.GrubSoup) || plr.HeldItem.type == ItemID.GrubSoup))
            {
                Beaten = true;
                break;
            }
        }
    }
}
