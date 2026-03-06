using Parterraria.Common;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using PathOfTerraria.Common.NPCs;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class ProjectileRainGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.Last;
    public override int MaxPlayTime => MinigameTimeInSeconds * 60;

    public int MinigameTimeInSeconds = 15;
    public float ProjectilesPerSecond = 2;
    public Point Left = Point.Zero;
    public Point Right = Point.Zero;
    public int ProjId = ProjectileID.Boulder;

    [HideFromEdit]
    private float _timer = 0;

    [HideFromEdit]
    private float _overallTimer = 0;

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

        Left = new Point(rectangle.Left, rectangle.Center.Y);
        Right = new Point(rectangle.Right, rectangle.Center.Y);
        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([], [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.EoCShield)], []);
        }
        else
            plr.QuickDismount();
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking() => MinigameRanking.ByLiving();

    public override void InternalUpdate()
    {
        _overallTimer++;

        if (_overallTimer > MaxPlayTime)
        {
            Beaten = true;
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        _timer += ProjectilesPerSecond;

        while (_timer > 60)
        {
            float factor = Main.rand.NextFloat();
            int x = (int)MathHelper.Lerp(Left.X, Right.X, factor);
            int y = (int)MathHelper.Lerp(Left.Y, Right.Y, factor);

            while (!WorldGen.SolidOrSlopedTile(x, y))
            {
                y--;
            }

            y += 2;

            int damage = ModeUtils.ProjectileDamage(30);
            Projectile.NewProjectile(new EntitySource_Minigame(WorldBoardSystem.Self.playingBoard, this), new Vector2(x, y).ToWorldCoordinates(), Vector2.Zero, ProjId, damage, 0);

            _timer -= 60;
        }
    }

    protected override void InternalSave(TagCompound tag)
    {
        tag.Add("maxTime", MinigameTimeInSeconds);
        tag.Add("left", Left);
        tag.Add("right", Right);
    }

    public override void LoadData(TagCompound tag)
    {
        MinigameTimeInSeconds = tag.GetInt("maxTime");
        Left = tag.Get<Point>("left");
        Right = tag.Get<Point>("right");
    }

    public override void ReadNetData(BinaryReader reader)
    {
        MinigameTimeInSeconds = reader.ReadInt16();
        ProjId = reader.ReadInt16();
        Left = new Point(reader.ReadInt32(), reader.ReadInt32());
        Right = new Point(reader.ReadInt32(), reader.ReadInt32());
    }

    public override void WriteNetData(BinaryWriter writer)
    {
        writer.Write((short)MinigameTimeInSeconds);
        writer.Write((short)ProjId);
        writer.Write(Left.X);
        writer.Write(Left.Y);
        writer.Write(Right.X);
        writer.Write(Right.Y);
    }
}
