using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

internal class PlainNode(BoardNode next, BoardNode prior) : EmptyNode(next, prior)
{
    public override void LandOn(Board board, Player player) => player.QuickSpawnItem(new EntitySource_Board(board), ItemID.GoldCoin, 3);
}
