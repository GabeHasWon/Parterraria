using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
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
        PriorityQueue<int, float> heightPrio = new();

        foreach (Player player in Main.ActivePlayers)
            heightPrio.Enqueue(player.whoAmI, player.position.Y);

        int[] who = new int[heightPrio.Count];
        int index = 0;

        while (heightPrio.Count > 0)
        {
            who[index] = heightPrio.Dequeue();
            index++;
        }
        
        return MinigameRanking.ByOrderAbsolute(who);
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
    }

    protected override void InternalSave(TagCompound tag) => tag.Add("maxTime", MinigameTimeInSeconds);
    public override void LoadData(TagCompound tag) => MinigameTimeInSeconds = tag.GetInt("maxTime");
}
