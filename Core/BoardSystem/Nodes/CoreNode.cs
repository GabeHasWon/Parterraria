using Parterraria.Common;
using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class CoreNode() : EmptyNode
{
    public override void PassBy(Board board, Player player)
    {
        if (CommonUtils.ConsumeItemFromInventory<AmethystCoin>(player, board.config.CoreCost, false))
            CommonUtils.SafelyAddItemToInv(player, ModContent.ItemType<CelestialCore>(), 1);
    }
}
