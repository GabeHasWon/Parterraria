using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class SplashArtGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.First;

    public int MinigameTimeInSeconds = 15;
    
    [HideFromEdit]
    private int _timer = 0;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 30 * 16)
        {
            rectangle.Width = 30 * 16;
            modified = true;
        }

        if (rectangle.Height < 25 * 16)
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
                [], [CommonUtils.AirItem, CommonUtils.AirItem, CommonUtils.AirItem, new Item(ItemID.EoCShield), 
                    new Item(ItemID.CloudinaBalloon), new Item(ItemID.LightningBoots)], []);
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        Dictionary<int, int> CountsByPaintId = [];

        for (int i = area.X / 16; i < area.Right / 16; ++i)
        {
            for (int j = area.Top / 16; j < area.Bottom / 16; ++j)
            {
                Tile tile = Main.tile[i, j];
                
                if (!CountsByPaintId.TryAdd(tile.WallColor, 1))
                    CountsByPaintId[tile.WallColor]++;
            }
        }

        bool max = HasMax(CountsByPaintId, out HashSet<int> players);

        if (max)
            return MinigameRanking.ByFirst(players.First());
        else
            return MinigameRanking.ByTie(players);
    }

    private static bool HasMax(Dictionary<int, int> countsByPaintId, out HashSet<int> player)
    {
        int currentMax = 0;
        player = [];

        foreach (var pair in countsByPaintId)
        {
            if (pair.Value > currentMax)
            {
                currentMax = pair.Value;
                player.Clear();
                player.Add(pair.Key);
            }
            else if (pair.Value == currentMax)
                player.Add(pair.Key);
        }

        return player.Count == 1;
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

        if (_timer > MinigameTimeInSeconds * 60)
        {
            Beaten = true;
            return;
        }

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active)
                PaintWall(plr, i + 1);
        }
    }

    private static void PaintWall(Player plr, int paintId)
    {
        Point16 pos = plr.position.ToTileCoordinates16();

        for (int i = 0; i < plr.width / 16 + 1; ++i)
        {
            for (int j = 0; j < plr.height / 16 + 1; ++j)
            {
                Tile tile = Main.tile[pos.X + i, pos.Y + j];
                tile.WallColor = (byte)paintId;
            }
        }
    }

    protected override void InternalSave(TagCompound tag) => tag.Add("maxTime", MinigameTimeInSeconds);
    public override void LoadData(TagCompound tag) => MinigameTimeInSeconds = tag.GetInt("maxTime");
}
