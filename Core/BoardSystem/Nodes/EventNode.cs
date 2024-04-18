using Parterraria.Core.BoardSystem.Events;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class EventNode() : EmptyNode
{
    public override void LandOn(Board board, Player player)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.rand.Next(Microevent.Microevents).Invoke(player);
        else if (player.whoAmI == Main.myPlayer)
            new SyncEventModule(Main.rand.Next(Microevent.Microevents.Count), player.whoAmI).Send(-1, -1, false);
    }
}
