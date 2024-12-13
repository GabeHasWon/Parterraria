using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Content.Items.Board.PartyItems;

namespace Parterraria.Core.BoardSystem.Nodes;

public class ItemNode() : EmptyNode
{
    public override void LandOn(Board board, Player player)
    {
        int[] items = [ModContent.ItemType<HighDice>(), ModContent.ItemType<DoubleDice>(), ModContent.ItemType<PartyMirror>(), ModContent.ItemType<BrokenDice>(),
            ModContent.ItemType<LowDice>()];
        CommonUtils.SafelyAddItemToInv(player, Main.rand.Next(items), 1);
    }
}
