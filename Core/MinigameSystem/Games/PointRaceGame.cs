using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class PointRaceGame : Minigame
{
    private static Asset<Texture2D> Flag = null;

    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll | MinigamePlayType.Team | MinigamePlayType.Duel;
    public override MinigameWinType WinType => MinigameWinType.InOrder;
    public override int MaxPlayTime => 0;

    Vector2 endPosition = Vector2.Zero;
    float distanceToWin = 60f;

    [HideFromEdit]
    private readonly Dictionary<int, int> RankingByWhoAmI = [];

    public override void Load() => Flag = ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Misc/CheckeredFlag");

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 20 * 16)
        {
            rectangle.Width = 40 * 16;
            modified = true;
        }

        if (rectangle.Height < 20 * 16)
        {
            rectangle.Height = 20 * 16;
            modified = true;
        }

        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([],
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.HermesBoots), new Item(ItemID.ShinyRedBalloon)], []);
        }
        else
            plr.SafeTeleport(playerStartLocation.ToWorldCoordinates());
    }

    public override void OnStart() => RankingByWhoAmI.Clear();

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking() => MinigameRanking.ByOrderAbsolute([.. RankingByWhoAmI.OrderBy(x => x.Value).Select(x => x.Key)]);

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!RankingByWhoAmI.ContainsKey(i) && plr.DistanceSQ(endPosition) < distanceToWin * distanceToWin)
                RankingByWhoAmI.Add(i, RankingByWhoAmI.Count);
        }

        if (RankingByWhoAmI.Count > Main.CurrentFrameFlags.ActivePlayersCount - 2)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (!RankingByWhoAmI.ContainsKey(player.whoAmI))
                {
                    RankingByWhoAmI.Add(player.whoAmI, RankingByWhoAmI.Count);
                    break;
                }
            }

            Beaten = true;
        }
    }

    protected override void InternalDraw(bool debug)
    {
        Vector2 position = endPosition - Main.screenPosition - new Vector2(0, MathF.Sin(Main.GameUpdateCount * 0.03f) * 5f);
        float alpha = 0.2f * MathF.Sin(Main.GameUpdateCount * 0.04f);
        Main.spriteBatch.Draw(Flag.Value, position, null, Color.White * (0.7f + alpha), 0f, Flag.Size() / 2f, 1f, SpriteEffects.None, 0);
    }

    protected override void InternalSave(TagCompound tag)
    {
        tag.Add("end", endPosition);
        tag.Add("distance", distanceToWin);
    }

    public override void LoadData(TagCompound tag)
    {
        endPosition = tag.Get<Vector2>("end");
        distanceToWin = tag.GetFloat("distance");
    }

    public override void ReadNetData(BinaryReader reader)
    {
        endPosition = reader.ReadVector2();
        distanceToWin = reader.ReadSingle();
    }

    public override void WriteNetData(BinaryWriter writer)
    {
        writer.WriteVector2(endPosition);
        writer.Write(distanceToWin);
    }

    protected override (object, LocalizedText)[] DebugDisplayPositions() => [(endPosition, Language.GetOrRegister(LocalizationPath + ".Positions.End"))];
}
