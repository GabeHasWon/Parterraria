using Parterraria.Content.Items.Board;
using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class StartNode() : EmptyNode
{
    public override void LandOn(Board board, Player player) => player.QuickSpawnItem(new EntitySource_Board(board), ModContent.ItemType<AmethystCoin>(), 5);
}
