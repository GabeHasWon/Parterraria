using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

internal class SimpleNode : BoardNode
{
    public override void LandOn(Board board, Player player)
    {
        player.QuickSpawnItem(new EntitySource_Board(board), ItemID.GoldCoin, 3);
    }

    public override BoardNode NextNode(Board board)
    {
        throw new System.NotImplementedException();
    }

    public override BoardNode PriorNode(Board board)
    {
        throw new System.NotImplementedException();
    }
}
