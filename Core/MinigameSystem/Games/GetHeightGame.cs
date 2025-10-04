using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class GetHeightGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.First;
    public override int MaxPlayTime => MinigameTimeInSeconds * 60;

    public int MinigameTimeInSeconds = 15;

    [HideFromEdit]
    private int _timer = 0;

    [HideFromEdit]
    public (int who, float minY)[] threeHighest = [(-1, Main.maxTilesY * 16), (-1, Main.maxTilesY * 16), (-1, Main.maxTilesY * 16)];

    private (Player player, float minY) Tallest(int index) => (Main.player[threeHighest[index].who], threeHighest[index].minY);

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 20 * 16)
        {
            rectangle.Width = 20 * 16;
            modified = true;
        }

        if (rectangle.Height < 50 * 16)
        {
            rectangle.Height = 50 * 16;
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
                    new Item(ItemID.LunarHook),
                    new Item(ItemID.LuckyHorseshoe)
                ], false, false);
        }
        else
        {
            plr.Center = playerStartLocation.ToWorldCoordinates();
            plr.RemoveAllGrapplingHooks();
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        List<(int, int)> order = [];
        int count = Math.Min(3, Main.CurrentFrameFlags.ActivePlayersCount);

        for (int i = 0; i < count; ++i)
        {
            order.Add((threeHighest[i].who, i));
        }

        return MinigameRanking.ByOrderContaining(order);
    }

    public override void OnStop()
    {
        for (int i = area.X / 16; i < area.Right / 16; ++i)
        {
            for (int j = area.Top / 16; j < area.Bottom / 16; ++j)
            {
                Tile tile = Main.tile[i, j];
                tile.WallColor = PaintID.None;
            }
        }
    }

    public override void InternalUpdate()
    {
        _timer++;

        if (_timer > MaxPlayTime)
        {
            Beaten = true;
            return;
        }

        foreach (Player player in Main.ActivePlayers)
        {
            if (threeHighest[2].who == -1)
            {
                threeHighest[2].who = player.whoAmI;
                threeHighest[2].minY = player.Center.Y;
                continue;
            }

            if (threeHighest[1].who == -1)
            {
                threeHighest[1].who = player.whoAmI;
                threeHighest[1].minY = player.Center.Y;
                continue;
            }

            if (threeHighest[0].who == -1)
            {
                threeHighest[0].who = player.whoAmI;
                threeHighest[0].minY = player.Center.Y;
                continue;
            }

            if (Tallest(0).minY > player.Center.Y)
            {
                threeHighest[0].who = player.whoAmI;
                threeHighest[0].minY = player.Center.Y;
                continue;
            }

            if (Tallest(1).minY > player.Center.Y)
            {
                threeHighest[1].who = player.whoAmI;
                threeHighest[1].minY = player.Center.Y;
                continue;
            }

            if (Tallest(2).minY > player.Center.Y)
            {
                threeHighest[2].who = player.whoAmI;
                threeHighest[2].minY = player.Center.Y;
                continue;
            }
        }
    }

    protected override void InternalSave(TagCompound tag) => tag.Add("maxTime", MinigameTimeInSeconds);
    public override void LoadData(TagCompound tag) => MinigameTimeInSeconds = tag.GetInt("maxTime");
}
