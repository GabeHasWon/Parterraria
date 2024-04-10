using Parterraria.Common;
using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class StartNode() : EmptyNode
{
    public override void LandOn(Board board, Player player) => CommonUtils.AddItemToInvOrSpawnIfOverfull(player, ModContent.ItemType<AmethystCoin>(), 5);
}
