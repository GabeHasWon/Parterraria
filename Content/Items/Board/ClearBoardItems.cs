using Parterraria.Core.BoardSystem;

namespace Parterraria.Content.Items.Board;

internal class ClearBoardItems : GlobalItem
{
    public override void UpdateInventory(Item item, Player player)
    {
        if (item.ModItem is IBoardClearItem && !WorldBoardSystem.PlayingParty)
            item.TurnToAir();
    }
}
