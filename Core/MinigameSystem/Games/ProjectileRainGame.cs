using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class ProjectileRainGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.Last;

    public int MinigameTimeInSeconds = 0;
    public float ProjectilesPerSecond = 2;
    public Point Left = Point.Zero;
    public Point Right = Point.Zero;
    public int ProjId = ProjectileID.WoodenArrowHostile;

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
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                [
                    new Item(ItemID.EoCShield),
                ], false);
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking() => MinigameRanking.ByLiving();

    public override void InternalUpdate()
    {
        _overallTimer++;

        if (_overallTimer > MinigameTimeInSeconds * 60)
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

            y++;

            Projectile.NewProjectile(new EntitySource_Minigame(WorldBoardSystem.Self.playingBoard, this), new Vector2(x, y).ToWorldCoordinates(), Vector2.Zero, ProjId, 40, 0);

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
}
