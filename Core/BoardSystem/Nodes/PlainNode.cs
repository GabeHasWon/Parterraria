using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class PlainNode() : EmptyNode
{
    public override void LandOn(Board board, Player player) => player.QuickSpawnItem(new EntitySource_Board(board), ItemID.GoldCoin, 3);
}
