using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class KingOfTheHillGame : Minigame
{
    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll;
    public override MinigameWinType WinType => MinigameWinType.First;
    public override int MaxPlayTime => MinigameTimeInSeconds * 60;

    public int MinigameTimeInSeconds = 15;

    [HideFromEdit]
    private int _timer = 0;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        RectangleMinimumTiles(ref rectangle, 30, 30, out bool modified);
        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([new Item(ItemID.Musket)], [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.LuckyHorseshoe), 
                new Item(ItemID.HermesBoots), new Item(ItemID.CloudinaBalloon)],
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.DualHook)]);
        }
        else
            plr.SafeTeleport(playerStartLocation.ToWorldCoordinates());
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        Dictionary<int, float> heightPrio = [];

        foreach (Player player in Main.ActivePlayers)
            heightPrio.Add(player.whoAmI, player.position.Y);

        return MinigameRanking.ByOrderAbsolute([.. heightPrio.OrderBy(x => x.Value).Select(x => x.Key)]);
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
    public override void WriteNetData(BinaryWriter writer) => writer.Write((byte)MinigameTimeInSeconds);
    public override void ReadNetData(BinaryReader reader) => MinigameTimeInSeconds = reader.ReadByte();
}
