using System;
using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Core.BoardSystem;

namespace Parterraria.Core.MinigameSystem;

[Serializable]
public class MinigameReward(string text, MinigameReward.Placement place)
{
    public enum Placement
    {
        First,
        Second,
        Third,
        Fourth,

        /// <summary>
        /// Any non-number placement, such as anything but 1st for <see cref="Minigame.MinigameWinType.First"/> games.
        /// </summary>
        Otherwise
    }

    public readonly string RewardText = text;
    public readonly Placement Place = place;

    public virtual void OnReward(Player player)
    {
        Placement placement = Place;

        if (placement is Placement.Otherwise or Placement.Fourth)
            return;

        CommonUtils.SafelyAddItemToInv<AmethystCoin>(player, placement switch
        {
            Placement.First => WorldBoardSystem.Self.playingBoard.config.CoinDeltaFromGames,
            Placement.Second => WorldBoardSystem.Self.playingBoard.config.CoinDeltaFromGames / 2,
            _ => 1,
        });
    }
}
