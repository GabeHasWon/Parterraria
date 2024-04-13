using Parterraria.Common;
using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class GoodNode() : EmptyNode
{
    public override void LandOn(Board board, Player player) => CommonUtils.SafelyAddItemToInv<AmethystCoin>(player, board.config.CoinDeltaFromNodes);
}
