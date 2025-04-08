using Parterraria.Content.NPCs;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class ReverseRaceGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.InOrder;

    Point endPosition = Point.Zero;
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
                ], false);
        }
    }

    public override void OnStart()
    {
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.HasItem(Mod.Find<ModItem>(nameof(GoldFaerie) + "Item").Type))
                return MinigameRanking.Ordered(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!RankingByWhoAmI.ContainsKey(i) && plr.DistanceSQ(endPosition.ToWorldCoordinates()) < distanceToWin * distanceToWin)
                RankingByWhoAmI.Add(i, RankingByWhoAmI.Count);
        }

        if (RankingByWhoAmI.Count >= Main.CurrentFrameFlags.ActivePlayersCount - 1)
            Beaten = true;
    }
}
