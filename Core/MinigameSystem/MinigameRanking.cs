using Parterraria.Common;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;

namespace Parterraria.Core.MinigameSystem;

#nullable enable

public class MinigameRanking
{
    public Dictionary<int, MinigameReward> Ranking = [];

    /// <summary>
    /// Ordered by first->last to display in order. Solves my need to manually order it.
    /// </summary>
    private IOrderedEnumerable<(int, MinigameReward)>? _displayRanking = null;

    /// <summary>
    /// Used for minigames where one player comes in first, everyone else comes in last (or, technically, comes in <see cref="MinigameReward.Placement.Otherwise"/>).
    /// </summary>
    /// <param name="winnerId">The winner of the game.</param>
    public static MinigameRanking ByFirst(int winnerId)
    {
        var rank = new MinigameRanking();
        rank.Ranking.Add(winnerId, new(Language.GetTextValue("Mods.Parterraria.Rankings.First"), MinigameReward.Placement.First));

        foreach (var item in Main.ActivePlayers)
        {
            if (item.whoAmI != winnerId)
                rank.Ranking.Add(item.whoAmI, new(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Otherwise));
        }

        return rank;
    }

    /// <summary>
    /// Used for minigames where living players come in first, everyone else comes in last (or, technically, comes in <see cref="MinigameReward.Placement.Otherwise"/>).<br/>
    /// <paramref name="binary"/> would mean players either get First or Last, exclusively. 
    /// Otherwise, sort by health lost - most health lost loses, dead people counting as an unconditional loss.<br/>
    /// <paramref name="disabled"/> uses the <see cref="MinigameDisablePlayer.Disabled"/> flag instead of checking for dead. This also forces <paramref name="binary"/> to true.
    /// </summary>
    public static MinigameRanking ByRemaining(bool binary = false, bool disabled = false)
    {
        var rank = new MinigameRanking();

        if (disabled)
            binary = true;

        if (binary)
        {
            foreach (var player in Main.ActivePlayers)
            {
                MinigameReward reward = (disabled ? player.GetModPlayer<MinigameDisablePlayer>().Disabled : player.dead) 
                    ? new(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Otherwise)
                    : new(Language.GetTextValue("Mods.Parterraria.Rankings.First"), MinigameReward.Placement.First);
                rank.Ranking.Add(player.whoAmI, reward);
            }
        }
        else
        {
            List<Player> players = [];

            foreach (var player in Main.ActivePlayers)
            {
                if (player.dead)
                    rank.Ranking.Add(player.whoAmI, new(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Otherwise));
                else
                    players.Add(player);
            }

            players = [.. players.OrderBy(x => x.statLifeMax2 - x.statLife)];
            int place = 0;

            foreach (Player player in players)
                rank.Ranking.Add(player.whoAmI, DetermineStandardOrderedPlacement(place++));
        }

        return rank;
    }

    /// <summary>
    /// A complete tie. Everyone is visually a "tie", but counts as <see cref="MinigameReward.Placement.Third"/> for rewards.
    /// </summary>
    public static MinigameRanking CompleteTie()
    {
        var rank = new MinigameRanking();

        foreach (var player in Main.ActivePlayers)
        {
            MinigameReward reward = new(Language.GetTextValue("Mods.Parterraria.Rankings.Tie"), MinigameReward.Placement.Third);
            rank.Ranking.Add(player.whoAmI, reward);
        }

        return rank;
    }

    /// <summary>
    /// Used for minigames where one or more players complete the win condition, or otherwise tie.
    /// </summary>
    /// <param name="players">The winners of the game.</param>
    /// <returns>The resultant ranking.</returns>
    public static MinigameRanking ByTie(HashSet<int> players)
    {
        var rank = new MinigameRanking();

        foreach (var player in Main.ActivePlayers)
        {
            MinigameReward reward = players.Contains(player.whoAmI) ? new(Language.GetTextValue("Mods.Parterraria.Rankings.Tie"), MinigameReward.Placement.First)
                : new(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Otherwise);
            rank.Ranking.Add(player.whoAmI, reward);
        }

        return rank;
    }

    public static MinigameRanking ByOrderAbsolute(int[] playerWhoAmIInOrderOfPlacement, HashSet<int>? forcedLast = null)
    {
        var rank = new MinigameRanking();

        for (int i = 0; i < playerWhoAmIInOrderOfPlacement.Length; i++)
        {
            int who = playerWhoAmIInOrderOfPlacement[i];
            rank.Ranking.Add(who, forcedLast?.Contains(who) is true ? new MinigameReward(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Otherwise) 
                : DetermineStandardOrderedPlacement(i));
        }

        return rank;
    }

    public static MinigameRanking ByOrderContaining(List<(int, int)> playerWhoAmIInOrderOfPlacement)
    {
        var rank = new MinigameRanking();

        foreach ((int who, int placement) in playerWhoAmIInOrderOfPlacement)
            rank.Ranking.Add(who, DetermineStandardOrderedPlacement(placement));

        return rank;
    }

    private static MinigameReward DetermineStandardOrderedPlacement(int placement) => placement switch
    {
        0 => new MinigameReward(Language.GetTextValue("Mods.Parterraria.Rankings.First"), MinigameReward.Placement.First),
        1 => new MinigameReward(Language.GetTextValue("Mods.Parterraria.Rankings.Second"), MinigameReward.Placement.Second),
        2 => new MinigameReward(Language.GetTextValue("Mods.Parterraria.Rankings.Third"), MinigameReward.Placement.Third),
        _ => new MinigameReward(Language.GetTextValue("Mods.Parterraria.Rankings.Last"), MinigameReward.Placement.Fourth),
    };

    internal void Draw(float alphaFade)
    {
        int step = 0;
        float scale = 1f;
        var color = Color.White * alphaFade;

        _displayRanking ??= Ranking.Select(x => (x.Key, x.Value)).OrderBy(x => x.Value.Place);

        foreach (var (who, rank) in _displayRanking)
        {
            var pos = Main.ScreenSize.ToVector2() / new Vector2(2f, 4f) + new Vector2(0, step++ * 40);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, pos, $"{Main.player[who].name}: {rank.RewardText} ", color, new(scale));

            scale *= 0.75f;

            if (step > 6)
                color *= 0.9f;
        }
    }

    internal void Reward(Player plr) => Ranking[plr.whoAmI].OnReward(plr);
}