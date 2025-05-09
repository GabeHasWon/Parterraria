using Parterraria.Content.NPCs;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Intrinsics;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

internal class PointRaceGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.InOrder;

    Vector2 endPosition = Vector2.Zero;
    float distanceToWin = 60f;

    [HideFromEdit]
    private readonly Dictionary<int, int> RankingByWhoAmI = [];

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
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                [
                    new Item(ItemID.LightningBoots),
                    new Item(ItemID.ShinyRedBalloon),
                ], false);
        }
        else
            plr.Center = playerStartLocation.ToWorldCoordinates();
    }

    public override void OnStart()
    {
        RankingByWhoAmI.Clear();
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking() => MinigameRanking.ByOrder([.. RankingByWhoAmI.Values]);

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!RankingByWhoAmI.ContainsKey(i) && plr.DistanceSQ(endPosition) < distanceToWin * distanceToWin)
                RankingByWhoAmI.Add(i, RankingByWhoAmI.Count);
        }

        if (RankingByWhoAmI.Count > Main.CurrentFrameFlags.ActivePlayersCount - 1)
            Beaten = true;
    }

    protected override void InternalSave(TagCompound tag) => tag.Add("end", endPosition);
    public override void LoadData(TagCompound tag) => endPosition = tag.Get<Vector2>("end");
}
