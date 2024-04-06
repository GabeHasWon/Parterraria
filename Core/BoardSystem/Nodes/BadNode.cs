using Parterraria.Content.Items.Board;
using System;

namespace Parterraria.Core.BoardSystem.Nodes;

public class BadNode() : EmptyNode
{
    public override void LandOn(Board board, Player player)
    {
        int coins = Math.Max(3, player.CountItem(ModContent.ItemType<AmethystCoin>(), 3));

        if (coins > 3)
            coins = 3;

        for (int i = 0; i < coins; i++) 
            player.ConsumeItem(ModContent.ItemType<AmethystCoin>());
    }
}
