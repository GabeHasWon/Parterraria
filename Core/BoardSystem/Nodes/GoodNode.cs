using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class GoodNode() : EmptyNode
{
    public override void LandOn(Board board, Player player) => player.QuickSpawnItem(new EntitySource_Board(board), ModContent.ItemType<AmethystCoin>(), 3);
}
