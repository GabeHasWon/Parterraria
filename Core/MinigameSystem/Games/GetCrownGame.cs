using Parterraria.Common;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.InventoryStorageSystem;
using System.IO;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class GetCrownGame : Minigame
{
    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll | MinigamePlayType.Duel;
    public override MinigameWinType WinType => MinigameWinType.First;
    public override int MaxPlayTime => 0;

    private Point _crownSpawnLocation = Point.Zero;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 30 * 16)
        {
            rectangle.Width = 30 * 16;
            modified = true;
        }

        if (rectangle.Height < 30 * 16)
        {
            rectangle.Height = 30 * 16;
            modified = true;
        }

        return modified;
    }

    public override void OnPlace() => _crownSpawnLocation = area.Center - new Point(0, 1);

    public override void OnStart()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Item.NewItem(new EntitySource_Minigame(WorldBoardSystem.Self.playingBoard, this), _crownSpawnLocation.ToWorldCoordinates(), ItemID.PlatinumCrown);
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([], [ ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.CloudinaBalloon)], 
                [new Item(ItemID.HermesBoots), ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.DualHook)]);
        }
        else
            plr.SafeTeleport(playerStartLocation.ToWorldCoordinates());
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override void OnStop()
    {
    }

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (PlayerHasCrown(plr))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.HasItem(ItemID.PlatinumCrown))
            {
                plr.armor[0] = new Item(ItemID.PlatinumCrown);
                plr.ConsumeItem(ItemID.PlatinumCrown);
            }

            if (PlayerHasCrown(plr))
            {
                Beaten = true;
                break;
            }
        }
    }

    private static bool PlayerHasCrown(Player plr) => plr.active && !plr.dead && 
        (plr.armor[0].type == ItemID.PlatinumCrown && !plr.armor[0].IsAir || plr.armor[10].type == ItemID.PlatinumCrown && !plr.armor[10].IsAir);
    protected override void InternalSave(TagCompound tag) => tag.Add(nameof(_crownSpawnLocation), _crownSpawnLocation);
    public override void LoadData(TagCompound tag) => _crownSpawnLocation = tag.Get<Point>(nameof(_crownSpawnLocation));

    public override void WriteNetData(BinaryWriter writer)
    {
        writer.Write(_crownSpawnLocation.X);
        writer.Write(_crownSpawnLocation.Y);
    }

    public override void ReadNetData(BinaryReader reader) => _crownSpawnLocation = new(reader.ReadInt32(), reader.ReadInt32());
    protected override (object, LocalizedText)[] DebugDisplayPositions() => [(_crownSpawnLocation, Language.GetOrRegister(LocalizationPath + ".Positions.Crown"))];
}
