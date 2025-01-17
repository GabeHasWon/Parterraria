using Microsoft.Xna.Framework.Graphics.PackedVector;
using Parterraria.Common;
using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Parterraria.Core.MinigameSystem;

internal class MinigameRanking
{
    public Dictionary<int, MinigameReward> Ranking = [];

    /// <summary>
    /// Used for minigames where one player comes in first, everyone else comes in last (or, technically, comes in <see cref="MinigameReward.Placement.Otherwise"/>).
    /// </summary>
    /// <param name="winnerId">The winner of the game.</param>
    /// <returns>The resultant ranking.</returns>
    public static MinigameRanking ByFirst(int winnerId)
    {
        var rank = new MinigameRanking();
        rank.Ranking.Add(winnerId, new("First", MinigameReward.Placement.First));

        foreach (var item in Main.ActivePlayers)
        {
            if (item.whoAmI != winnerId)
                rank.Ranking.Add(item.whoAmI, new("Last", MinigameReward.Placement.Otherwise));
        }

        return rank;
    }

    internal void Draw(float alphaFade)
    {
        int step = 0;
        float scale = 1f;
        var color = Color.White * alphaFade;

        foreach (var (who, rank) in Ranking)
        {
            var pos = Main.ScreenSize.ToVector2() / new Vector2(2f, 4f) + new Vector2(0, step++ * 40);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, pos, $"{Main.player[who].name}: {rank.RewardText}", color, new(scale));

            scale *= 0.75f;

            if (step > 6)
                color *= 0.9f;
        }
    }

    internal void Reward(Player plr) => Ranking[plr.whoAmI].OnReward(plr);
}