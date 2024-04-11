using Parterraria.Common;
using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class CoreNode() : EmptyNode
{
    public override void PassBy(Board board, Player player)
    {
        if (CommonUtils.ConsumeItemFromInventory(player, ModContent.ItemType<AmethystCoin>(), 20, false))
            CommonUtils.AddItemToInvOrSpawnIfOverfull(player, ModContent.ItemType<CelestialCore>(), 1);
    }
}
