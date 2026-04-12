using Parterraria.Common;
using Parterraria.Content.Items.Board;

namespace Parterraria.Core.BoardSystem.Nodes;

public class BadNode() : EmptyNode
{
    public override MinigameReferral Referral => MinigameReferral.PvP;

    public override void LandOn(Board board, Player player) => CommonUtils.ConsumeItemFromInventory<AmethystCoin>(player, board.config.CoinDeltaFromNodes, true);
}
