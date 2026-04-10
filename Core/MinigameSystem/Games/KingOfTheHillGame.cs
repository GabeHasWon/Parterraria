using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class KingOfTheHillGame : Minigame
{
    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll | MinigamePlayType.Team;
    public override MinigameWinType WinType => MinigameWinType.InOrder;
    public override int MaxPlayTime => MinigameTimeInSeconds * 60;
    public override bool PvPGame => true;

    private static Asset<Texture2D> Circle = null;

    public int MinigameTimeInSeconds = 15;

    public Point HoldPlace = Point.Zero;
    public float HoldDistance = 200f;

    [HideFromEdit]
    private readonly int[] _perPlayerPoints = new int[Main.maxPlayers];

    [HideFromEdit]
    private int _timer = 0;

    public override void Load() => Circle = ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Misc/FadeCircle");

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        RectangleMinimumTiles(ref rectangle, 30, 30, out bool modified);
        return modified;
    }

    public override void OnPlace() => HoldPlace = area.Center;

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([new Item(ItemID.Musket), new Item(ItemID.BreakerBlade), new Item(ItemID.MusketBall, 999)], 
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.LuckyHorseshoe), new Item(ItemID.HermesBoots), new Item(ItemID.CloudinaBalloon)],
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.DualHook)]);
        }
        else
            plr.SafeTeleport(playerStartLocation.ToWorldCoordinates());
    }

    public override void OnStart()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
            _perPlayerPoints[i] = 0;
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        Dictionary<int, float> prio = [];
        HashSet<int> forcedLast = [];

        foreach (Player player in Main.ActivePlayers)
        {
            prio.Add(player.whoAmI, _perPlayerPoints[player.whoAmI]);

            if (_perPlayerPoints[player.whoAmI] == 0)
                forcedLast.Add(player.whoAmI);
        }

        if (prio.Count == 0)
            return MinigameRanking.CompleteTie();

        return MinigameRanking.ByOrderAbsolute([.. prio.OrderBy(x => x.Value).Select(x => x.Key)], forcedLast);
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
            if (player.DistanceSQ(HoldPlace.ToWorldCoordinates()) < HoldDistance * HoldDistance)
                _perPlayerPoints[player.whoAmI]++;
        }
    }

    protected override void InternalDraw(bool debug)
    {
        Vector2 position = HoldPlace.ToWorldCoordinates() - Main.screenPosition - new Vector2(0, MathF.Sin(Main.GameUpdateCount * 0.03f) * 5f);
        float alpha = 0.1f * MathF.Sin(Main.GameUpdateCount * 0.04f);
        Main.spriteBatch.Draw(Circle.Value, position, null, Color.Red * (0.3f + alpha), 0f, Circle.Size() / 2f, HoldDistance / Circle.Width(), SpriteEffects.None, 0);
    }

    protected override void InternalSave(TagCompound tag)
    {
        tag.Add("maxTime", MinigameTimeInSeconds);
        tag.Add("hold", HoldPlace);
        tag.Add("dist", HoldDistance);
    }

    public override void LoadData(TagCompound tag)
    {
        MinigameTimeInSeconds = tag.GetInt("maxTime");
        HoldPlace = tag.Get<Point>("hold");
        HoldDistance = tag.GetFloat("dist");
    }

    public override void WriteNetData(BinaryWriter writer)
    {
        writer.Write((byte)MinigameTimeInSeconds);
        writer.Write((short)HoldPlace.X);
        writer.Write((short)HoldPlace.Y);
        writer.Write((Half)HoldDistance);
    }

    public override void ReadNetData(BinaryReader reader)
    {
        MinigameTimeInSeconds = reader.ReadByte();
        HoldPlace = new Point(reader.ReadInt16(), reader.ReadInt16());
        HoldDistance = (float)reader.ReadHalf();
    }
}
